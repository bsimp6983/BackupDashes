using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using DowntimeCollection_Demo.Classes;

namespace DowntimeCollection_Demo
{
    public class DigestEmailView
    {
        public int Id { get; set; }
        public string Client { get; set; }
        public string Email { get; set; }
        public bool IsDaily { get; set; }
        public bool IsWeekly { get; set; }
        public List<string> Lines { get; set; }
    }

    public class DigestEmailHelper
    {
        public static DigestEmail ToDigestEmail(DigestEmailView view)
        {
            DigestEmail email = new DigestEmail();

            if (view != null)
            {
                email.Id = view.Id;
                email.Email = view.Email;
                email.IsDaily = view.IsDaily;
                email.IsWeekly = view.IsWeekly;
            }
            return email;
        }

        public static DigestEmailView ToDigestEmailView(DigestEmail email)
        {
            DigestEmailView view = new DigestEmailView();

            if (email != null)
            {
                view.Id = email.Id;
                view.Email = email.Email;
                view.Lines = email.DigestEmailLines.Select(o => o.Line).ToList();
                view.IsDaily = email.IsDaily;
                view.IsWeekly = email.IsWeekly;
            }

            return view;
        }

        public static List<string> GetClientLines(string client)
        {
            List<string> lines = new List<string>();

            using (DB db = new DB(DBHelper.GetConnectionString()))
            {
                lines = (from o in db.LineSetups
                            where o.DataCollectionNode.Client == client
                            group o by o.Line
                             into g
                             orderby g.Key ascending
                             select g.Key).ToList();

                return lines;
            }
        }

        public static List<DigestEmailView> GetClientEmailViews(string client)
        {
            List<DigestEmailView> views = new List<DigestEmailView>();

            using (DB db = new DB(DBHelper.GetConnectionString()))
            {
                var lines = (from o in db.DigestEmailLines
                             join b in db.DigestEmails
                             on o.DigestEmail.Id equals b.Id
                             where b.Client == client
                             select new { Line = o.Line, EmailId = o.DigestEmail.Id });

                views = (from o in db.DigestEmails
                         where o.Client == client
                         select new DigestEmailView { Id = o.Id, Client = o.Client, Email = o.Email, IsWeekly = o.IsWeekly, IsDaily = o.IsDaily }).ToList();

                foreach (DigestEmailView view in views)
                {
                    view.Lines = (from o in lines
                                  where o.EmailId == view.Id
                                  select o.Line).ToList();
                }

                return views;
            }
        }

        public static List<DigestEmail> GetClientEmails(string client)
        {
            List<DigestEmail> emails = new List<DigestEmail>();

            using (DB db = new DB(DBHelper.GetConnectionString()))
            {
                emails = (from o in db.DigestEmails
                          where o.Client == client
                          select o).ToList();

                return emails;
            }
        }

        public static DigestEmail GetEmail(int id)
        {
            using (DB db = new DB(DBHelper.GetConnectionString()))
            {
                DigestEmail email = (from o in db.DigestEmails
                                     where o.Id == id
                                     select o).FirstOrDefault();

                return email;
            }
        }

        public static DigestEmailView GetEmailView(int id)
        {
            DigestEmail email = GetEmail(id);

            if (!email.DigestEmailLines.IsLoaded)
                email.DigestEmailLines.Load();

            DigestEmailView view = new DigestEmailView();

            if (email != null)
            {
                view = ToDigestEmailView(email);
            }

            return view;
        }

        public static bool Delete(int id)
        {
            using (DB db = new DB(DBHelper.GetConnectionString()))
            {
                DigestEmail email = (from o in db.DigestEmails
                                     where o.Id == id
                                     select o).FirstOrDefault();

                if (email != null)
                {
                    db.DeleteObject(email);

                    return db.SaveChanges() > 0;
                }

            }

            return false;
        }

        public static bool DeleteFromClient(int id, string client)
        {
            using (DB db = new DB(DBHelper.GetConnectionString()))
            {
                DigestEmail email = (from o in db.DigestEmails
                                     where o.Id == id
                                     && o.Client == client
                                     select o).FirstOrDefault();

                if (email != null)
                {
                    email.DigestEmailLines.Load();

                    foreach (DigestEmailLine line in email.DigestEmailLines.ToList())
                        db.DeleteObject(line);

                    db.DeleteObject(email);

                    return db.SaveChanges() > 0;
                }

            }

            return false;
        }

        public static bool Update(DigestEmailView view)
        {
            using (DB db = new DB(DBHelper.GetConnectionString()))
            {
                DigestEmail email = (from o in db.DigestEmails
                                     where o.Id == view.Id
                                     select o).FirstOrDefault();

                if (email != null)
                {
                    email.Email = view.Email;
                    email.IsDaily = view.IsDaily;
                    email.IsWeekly = view.IsWeekly;

                    AddLines(email.Id, view.Lines);

                    return db.SaveChanges() > 0;
                }

            }

            return false;
        }

        public static DigestEmailView Insert(DigestEmailView view)
        {
            using (DB db = new DB(DBHelper.GetConnectionString()))
            {
                DigestEmail email = new DigestEmail();

                if (email != null)
                {
                    email.Email = view.Email;
                    email.Client = view.Client;
                    email.IsWeekly = view.IsWeekly;
                    email.IsDaily = view.IsDaily;

                    db.AddToDigestEmails(email);

                    db.SaveChanges();

                    if(email.Id > 0)
                        AddLines(email.Id, view.Lines);

                    return ToDigestEmailView(email);
                }

            }

            return view;
        }

        public static List<string> AddLines(int emailId, List<string> lines)
        {
            using (DB db = new DB(DBHelper.GetConnectionString()))
            {
                DigestEmail email = (from o in db.DigestEmails
                                     where o.Id == emailId
                                     select o).FirstOrDefault();

                if (email != null)
                {
                    email.DigestEmailLines.Load();

                    foreach (string line in lines)
                    {
                        DigestEmailLine emailLine = (from o in email.DigestEmailLines
                                                     where o.Line == line
                                                     select o).FirstOrDefault();

                        if (emailLine == null)
                        {
                            AddLine(email.Id, line);//If new, add it
                        }
                    }

                    foreach (DigestEmailLine emailLine in email.DigestEmailLines)
                    {
                        string line = (from o in lines
                                       where o == emailLine.Line
                                       select o).FirstOrDefault();

                        if (string.IsNullOrEmpty(line))
                        {
                            DeleteLine(email.Id, emailLine.Line);
                        }
                    }

                    email.DigestEmailLines.Load();//reload from db

                    return email.DigestEmailLines.Select(o => o.Line).ToList();
                }

            }

            return new List<string>();

        }

        public static DigestEmailLine AddLine(int emailId, string line)
        {
            using (DB db = new DB(DBHelper.GetConnectionString()))
            {
                DigestEmail email = (from o in db.DigestEmails
                                     where o.Id == emailId
                                     select o).FirstOrDefault();

                if (email != null)
                {
                    email.DigestEmailLines.Load();

                    DigestEmailLine emailLine = (from o in email.DigestEmailLines
                                                 where o.Line == line
                                                 select o).FirstOrDefault();

                    if (emailLine == null)
                    {
                        emailLine = new DigestEmailLine();
                        emailLine.Line = line;
                        emailLine.DigestEmail = email;

                        db.AddToDigestEmailLines(emailLine);

                        db.SaveChanges();

                        return emailLine;
                    }
                }
            }

            return null;
        }

        public static void DeleteLine(int emailId, string line)
        {
            using (DB db = new DB(DBHelper.GetConnectionString()))
            {
                DigestEmail email = (from o in db.DigestEmails
                                     where o.Id == emailId
                                     select o).FirstOrDefault();

                if (email != null)
                {
                    email.DigestEmailLines.Load();

                    DigestEmailLine emailLine = (from o in email.DigestEmailLines
                                                 where o.Line == line
                                                 select o).FirstOrDefault();

                    if (emailLine != null)
                    {
                        db.DeleteObject(emailLine);

                        db.SaveChanges();
                    }
                }
            }
        }


    }
}