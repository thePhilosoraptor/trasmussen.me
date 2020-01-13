jQuery(document).ready(function( $ ) {

  // Smooth scroll for the menu and links with .scrollto classes
  $('.smothscroll').on('click', function(e) {
    e.preventDefault();
    if (location.pathname.replace(/^\//, '') == this.pathname.replace(/^\//, '') && location.hostname == this.hostname) {
      var target = $(this.hash);
      if (target.length) {

        $('html, body').animate({
          scrollTop: target.offset().top - 62
        }, 1500, 'easeInOutExpo');
      }
    }
  });

  $('.carousel').carousel({
    interval: 3500
  });

  // JavaScript Chart
  var doughnutData = [{
      value: 55,
      color: "#1abc9c"
    },
    {
      value: 45,
      color: "#ecf0f1"
    }
  ];
  var myDoughnut = new Chart(document.getElementById("javascript").getContext("2d")).Doughnut(doughnutData);

  // C++ Chart
  var doughnutData = [{
    value: 85,
    color: "#1abc9c"
  },
  {
    value: 15,
    color: "#ecf0f1"
  }
  ];
  var myDoughnut = new Chart(document.getElementById("cpp").getContext("2d")).Doughnut(doughnutData);

  // C# Chart
  var doughnutData = [{
    value: 70,
    color: "#1abc9c"
  },
  {
    value: 30,
    color: "#ecf0f1"
  }
  ];
  var myDoughnut = new Chart(document.getElementById("cs").getContext("2d")).Doughnut(doughnutData);

  // C Chart
  var doughnutData = [{
    value: 55,
    color: "#1abc9c"
  },
  {
    value: 45,
    color: "#ecf0f1"
  }
  ];
  var myDoughnut = new Chart(document.getElementById("c").getContext("2d")).Doughnut(doughnutData);

  // Java Chart
  var doughnutData = [{
    value: 80,
    color: "#1abc9c"
  },
  {
    value: 20,
    color: "#ecf0f1"
  }
  ];
    var myDoughnut = new Chart(document.getElementById("java").getContext("2d")).Doughnut(doughnutData);

  // Python
  var doughnutData = [{
    value: 30,
    color: "#1abc9c"
  },
  {
    value: 70,
    color: "#ecf0f1"
  }
  ];
  var myDoughnut = new Chart(document.getElementById("python").getContext("2d")).Doughnut(doughnutData);

  // Object-Oriented Programming Chart
  var doughnutData = [{
    value: 95,
    color: "#1abc9c"
  },
  {
    value: 05,
    color: "#ecf0f1"
  }
  ];
  var myDoughnut = new Chart(document.getElementById("oop").getContext("2d")).Doughnut(doughnutData);

  // Agile
  var doughnutData = [{
    value: 75,
    color: "#1abc9c"
  },
  {
    value: 25,
    color: "#ecf0f1"
  }
  ];
  var myDoughnut = new Chart(document.getElementById("agile").getContext("2d")).Doughnut(doughnutData);

  // Algorithm Development
  var doughnutData = [{
    value: 90,
    color: "#1abc9c"
  },
  {
    value: 10,
    color: "#ecf0f1"
  }
  ];
  var myDoughnut = new Chart(document.getElementById("algo").getContext("2d")).Doughnut(doughnutData);

});
