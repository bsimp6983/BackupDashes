using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;

namespace DowntimeCollection_Demo
{
    public partial class CnmPager : System.Web.UI.UserControl
    {
        protected void Page_Load(object sender, EventArgs e)
        {

        }

        int _itemCount;
        int _pageSize;
        int _pageIndex;
        string _urlFormat;

        /// <summary>
        /// 形如 /list.aspx?page={0} 的字符串，其中，{0}会被替换为页索引
        /// </summary>
        public string UrlFormat
        {
            get { return _urlFormat; }
            set { _urlFormat = value; }
        }

        public int PageIndex
        {
            get
            {
                //if (_pageIndex < 0)
                //{
                //    return 0;
                //}
                //if (_pageIndex >= PageCount)
                //{
                //    return PageCount - 1;
                //}
                return _pageIndex;
            }
            set { _pageIndex = value; }
        }

        public int ItemCount
        {
            get { return _itemCount; }
            set { _itemCount = value; }
        }

        public int PageSize
        {
            get { return _pageSize; }
            set { _pageSize = value; }
        }

        public int PageCount
        {
            get
            {
                ChunkHelper chunkAlg = new ChunkHelper(ItemCount, PageSize);
                return chunkAlg.ChunkCount == 0 ? 1 : chunkAlg.ChunkCount;
            }
        }

    }
}