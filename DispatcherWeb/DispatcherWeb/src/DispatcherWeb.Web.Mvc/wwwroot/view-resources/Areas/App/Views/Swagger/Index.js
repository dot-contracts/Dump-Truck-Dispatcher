(function () {
    $(function () {
        const ui = SwaggerUIBundle({
            url: "/swagger/v1/swagger.json",
            dom_id: '#swagger-ui',
            presets: [
                SwaggerUIBundle.presets.apis,
                SwaggerUIStandalonePreset
            ],
            layout: "BaseLayout"
        });
    });
})();
