'use strict';

/* Controllers */
function TestSuiteCtrl($scope, $http) {
	$http.get('gettestsuite.json').success(function (data) {
		$scope.assemblypath = data.assemblypath;
		$scope.testresultpath = data.testresultpath;
	});
}

function TestListCtrl($scope, $http) {
	$http.get('getcategories.json').success(function (data) {
		$scope.categories = data.categories;
	});

	$http.get('gettests.json').success(function (data) {
		$scope.fixtures = data.fixtures;
	});

	var selected = $scope.selected = [];
	$scope.categorygroup = '';
	$scope.updateSelection = function ($event, name) {
		var checkbox = $event.target;
		var action = (checkbox.checked ? 'add' : 'remove');
		if (action == 'add' & selected.indexOf(name) == -1) selected.push(name);
		if (action == 'remove' && selected.indexOf(name) != -1) selected.splice(selected.indexOf(name), 1);
		var categorygroup = [];
		for (var i = 0; i < selected.length; i++) {
			categorygroup.push(encodeURIComponent(selected[i]));
		}
		$scope.categorygroup = categorygroup.join();
	};
}

function FixtureDetailCtrl($scope, $http, $timeout, $routeParams) {
	var name = $routeParams.name;
	var url = 'runfixture.json?name=' + name;
	runTests($scope, $http, $timeout, url);
}

function TestDetailCtrl($scope, $http, $timeout, $routeParams) {
	var id = $routeParams.id;
	var url;
	if (id == 'all') {
		url = 'runtests.json';
	} else {
		url = 'runtest.json?id=' + id;
	}
	runTests($scope, $http, $timeout, url);
}

function CategoryDetailCtrl($scope, $http, $timeout, $routeParams) {
	var name = $routeParams.name;
	var url = 'runcategories.json?name=' + name;
	runTests($scope, $http, $timeout, url);
}

function runTests($scope, $http, $timeout, url) {
	$scope.counter = 0;
	$scope.onTimeout = function () {
		$http.get("getrunnerstatus.json").success(function (data) {
			$scope.counter = data.counter;
			if (data.active) {
				var perc = data.counter + '%';
				$('.bar').css('width', perc);
				mytimeout = $timeout($scope.onTimeout, 1000);
			} else {
				$('.progress').removeClass('active');
			}
		});
	};
	
	var mytimeout = $timeout($scope.onTimeout, 1000);

	$scope.stop = function () {
		$timeout.cancel(mytimeout);
		$http.get("cancel.json");
	};
	
	$http.get(url).success(function (data) {
		$scope.message = data.message;
		$scope.fixtures = data.fixtures;
		$scope.errorlist = data.errorlist;
		$scope.ignoredlist = data.ignoredlist;
		$scope.textoutput = data.textoutput;
	});
}
