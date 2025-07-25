(function () {

    abp.helper ??= {};
    abp.helper.graphs ??= {};

    abp.helper.graphs.initBarChart = async function initBarChart(options) {
        options.container = $(options.container);
        options.loadingContainer = $(options.loadingContainer);
        options.barOptions = options.barOptions || {};
        if (!options.getDataAsync) {
            return;
        }

        if (!options.container.length) {
            return;
        }

        options.container.hide();
        options.loadingContainer.show();

        //added maxXLabelLength option to truncate the extra long labels, but keep showing the full label in the hover text
        if (options.barOptions.maxXLabelLength && options.barOptions.xkey) {
            let originalXLabelFormat = options.barOptions.xLabelFormat;
            let originalHoverCallback = options.barOptions.hoverCallback;

            options.barOptions.xLabelFormat = function (x) {
                let text = originalXLabelFormat ? originalXLabelFormat(x) : x.label;
                return abp.utils.truncate(text, options.barOptions.maxXLabelLength, true);
            };

            options.barOptions.hoverCallback = function (index, barOptions, content, row) {
                let finalContent = $(content);
                finalContent.eq(0).text(row[options.barOptions.xkey]);

                if (originalHoverCallback) {
                    finalContent = originalHoverCallback(index, barOptions, finalContent, row);
                }

                return finalContent;
            };
        }

        var updateGraphHeight = function (data) {
            let fallbackHeight = options.height || 342;
            let height = fallbackHeight;
            if (options.barOptions.horizontal) {
                const rowHeight = options.rowHeight || 30; //px
                const footerHeight = 56;
                const topPadding = 26;
                height = data.length ? data.length * rowHeight + footerHeight + topPadding : fallbackHeight;
            }
            options.container.css('height', height + 'px');
            return height;
        }

        var BarChart = function (element) {
            var init = function (data) {
                return new Morris.Bar({
                    element: element,
                    fillOpacity: 1,
                    data: data,
                    lineColors: ['#399a8c'],
                    hideHover: 'auto',
                    resize: true,
                    ...options.barOptions
                });
            };

            var refresh = async function () {
                let result = await options.getDataAsync();
                this.draw(result);
            };

            var draw = function (data) {
                let newHeight = updateGraphHeight(data);
                if (data.length) {
                    if (!this.graph) {
                        this.graph = init(data);
                    } else {
                        this.graph.setData(data);
                        this.graph.redraw();
                        this.graph.el.find('svg').attr('height', newHeight);
                    }
                } else {
                    console.warn("An empty array was passed to Morris Bar graph");
                }
            };

            return {
                draw: draw,
                refresh: refresh
            };
        };
        var data = await options.getDataAsync();

        options.container.empty();
        options.container.show();

        var chart = new BarChart(options.container);
        chart.draw(data);

        options.loadingContainer.hide();

        return chart;
    };

    abp.helper.graphs.initDonutChart = async function initDonutChart(options) {
        options.container = $(options.container);
        options.highlightedDataIndex = options.highlightedDataIndex || 0;
        options.donutOptionsGetter = options.donutOptionsGetter || (() => { return {} });
        options.hasData = options.hasData || ((result) => true);

        if (!options.container.length || !options.getDataAsync) {
            return;
        }

        var DonutChart = function (options) {
            let donutElement = options.container.find('.dashboard-donut-chart');
            let legendContainer = options.container.find('.donut-legend');

            var init = function (data) {
                let donutOptions = {
                    element: donutElement,
                    resize: true,
                    ...options.donutOptionsGetter(data)
                };
                return new Morris.Donut(donutOptions);
            }

            var draw = function (data) {
                donutElement.show();
                if (!this.graph) {
                    this.graph = init(data);
                    this.graph.select(options.highlightedDataIndex);

                    // Add Legends
                    this.graph.data.forEach(function (item, i) {
                        var legendItem = $('<span></span>').text(item['label']).prepend('<i>&nbsp;</i>');
                        legendItem.find('i').css('backgroundColor', item.color);
                        legendContainer.append(legendItem);
                    });

                } else {
                    this.graph.setData(data);
                    this.graph.redraw();
                    this.graph.select(options.highlightedDataIndex);
                }
            };

            return {
                draw: draw
            };
        };

        try {
            updateDonutChartContainerVisibility(options.container);
            let data = await options.getDataAsync();
            let hasData = options.hasData(data);
            if (options.gotDataCallback) {
                options.gotDataCallback(data, hasData);
            }
            updateDonutChartContainerVisibility(options.container, hasData);
            if (!hasData) {
                return;
            }
            let chart = new DonutChart(options);
            chart.draw(data);
        }
        catch (e) {
            options.container.hide();
            throw e;
        }
    }

    function updateDonutChartContainerVisibility(container, hasData) {
        let loading = hasData === undefined;
        container.find('.data-loading').toggle(loading);
        //container.find('.donut-chart-header-text').toggle(!loading);
        container.find('.no-data').toggle(!loading && !hasData);
        container.find('.has-data').toggle(!loading && !!hasData);
    }
})();
