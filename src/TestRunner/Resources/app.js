'use strict';

/* App Module */
angular.module('runner', ['ngRoute']).
	config(['$routeProvider', function ($routeProvider) {
		$routeProvider.
			when('/', { templateUrl: 'list.html', controller: 'TestListCtrl' }).
			when('/fixture/:name', { templateUrl: 'results.html', controller: 'FixtureDetailCtrl' }).
			when('/test/:id', { templateUrl: 'results.html', controller: 'TestDetailCtrl' }).
			when('/category/:name', { templateUrl: 'results.html', controller: 'CategoryDetailCtrl' }).
			otherwise({ redirectTo: '/' });
	} ])
;

