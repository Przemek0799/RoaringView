window.chartFunctions = {
    createLineChart: function (elementId, data, options) {
        console.log("createLineChart called with elementId:", elementId);
        var ctx = document.getElementById(elementId);

        if (!ctx) {
            console.error("Element not found. Current DOM:", document.body.innerHTML);
            return;
        }

        console.log("Context obtained, creating chart...");
        new Chart(ctx.getContext('2d'), {
            type: 'line',
            data: data,
            options: options
        });
    },
      createPieChart: function (elementId, data, options) {
        console.log("createPieChart called with elementId:", elementId);
        var ctx = document.getElementById(elementId);

        if (!ctx) {
            console.error("Element not found for Pie Chart. Current DOM:", document.body.innerHTML);
            return;
        }

        console.log("Context obtained for Pie Chart, creating chart...");
        new Chart(ctx.getContext('2d'), {
            type: 'pie',
            data: data,
            options: options
        });
    },

    createBarChart: function (elementId, data, options) {
        console.log("createBarChart called with elementId:", elementId);
        var ctx = document.getElementById(elementId);

        if (!ctx) {
            console.error("Element not found for Bar Chart. Current DOM:", document.body.innerHTML);
            return;
        }

        console.log("Context obtained for Bar Chart, creating chart...");
        new Chart(ctx.getContext('2d'), {
            type: 'bar',
            data: data,
            options: options
        });
    }
};

