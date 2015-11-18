'use strict';
// Declare app level module which depends on filters, and services
var Module = angular.module('DigestEmails', ['ui', 'ngResource']).
    config(['$routeProvider', '$locationProvider', function ($routeProvider, $locationProvider) {
        $locationProvider.html5Mode(true);

        $routeProvider.when('/DigestEmails.aspx/', { controller: ctrlBase });

    } ]);

    function ctrlBase($scope, $location, $http, DigestEmail) {
        $scope.originalEmails = [];

        $scope.emails = DigestEmail.query(function (emails) {
            $scope.originalEmails = angular.copy(emails);
        });

        $scope.sendEmail = function(){
            $http.post('Service.ashx?op=sendclientdailyemail');
        };

        $http.get('DigestEmails.ashx?l=true').success(function (lines) {
            $scope.lines = lines;
        });

        $scope.addEmail = function () {
            $scope.emails.push({
                Id: 0,
                Email: '',
                Lines: []
            });
        };

        $scope.save = function () {
            angular.forEach($scope.emails, function (email) {

                if (email.Id) {
                    angular.forEach($scope.originalEmails, function (oE) {
                        if (oE.Id == email.Id) {
                            if (oE.Email != email.Email || oE.Lines != email.Lines) {
                                email.update();
                            }
                        }
                    });
                } else {
                    DigestEmail.save(email, function(e){
                        email.Id = e.Id;
                    });
                }
            });
        };

        $scope.delete = function(email){

            var ans = true;
            if(email){
                if(email.Id > 0){
                    ans = confirm('Are you sure?');

                    if(ans)
                        email.destroy({id: email.Id});                
                }
                
                if(ans){
                    for(var x = 0; x < $scope.emails.length; x++){
                        if($scope.emails[x] == email){
                            $scope.emails.splice(x, 1);
                        }
                    }       
                }       
            }
        }
    }

    Module.factory('DigestEmail', function ($resource) {

        var DigestEmail = $resource('DigestEmails.ashx',
                        {}, {
                            update: { method: 'POST', params:{
                                'update': true
                            } },
                            remove: { method: 'POST', params: {
                                    'delete': true
                                }
                            }
                        }
                    );


        DigestEmail.prototype.collection = [];

        var _query = angular.copy(DigestEmail.query); //Original query
        var _get = angular.copy(DigestEmail.get); //Original get

        DigestEmail.query = function () {
            var args = arguments[0] || null;
            var cb = arguments[1] || null;

            if (angular.isFunction(args)) {
                cb = args;
                args = null;
            }

            return _query(args, function (objs) {
                DigestEmail.collection = objs;

                if (angular.isFunction(cb)) {
                    cb(objs);
                }

            });
        };

        DigestEmail.get = function () {
            var args = arguments[0] || null;
            var cb = arguments[1] || null;

            if (angular.isFunction(args)) {
                cb = args;
                args = null;
            }

            return _get(args, function (obj) {
                angular.forEach(DigestEmail.collection, function (c) {
                    if (obj.id == c.id) {
                        c = obj;
                        return false;
                    }
                });

                if (angular.isFunction(cb)) {
                    cb(obj);
                }

            });
        };

        DigestEmail.prototype.update = function (cb) {
            return DigestEmail.update({ Id: this.Id },
            angular.extend({}, this, { Id: this.Id }), cb);
        };

        DigestEmail.prototype.destroy = function (cb) {
            return DigestEmail.remove({ Id: this.Id }, cb);
        };

        return DigestEmail;
    });