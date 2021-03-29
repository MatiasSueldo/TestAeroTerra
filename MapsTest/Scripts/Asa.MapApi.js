// https://developers.arcgis.com/javascript/3/jsapi/
var mapApp = {
    map : undefined
}

mapApp.getPOIs = function (callback) {
    $.get("../api/pois", function (data) {
        console.log(data)
        if (callback) callback(data.pois);
    });
}

mapApp.getCats = function (callback) {
    $.get("../api/categories", function (data) {
        console.log(data)
        if (callback) callback(data.categories);
    });
}



mapApp.initMap = function (callback) {
    mapApp.map;

    require(["esri/map", "esri/symbols/SimpleMarkerSymbol", "esri/layers/GraphicsLayer", "dojo/domReady!", "esri/symbols/PictureMarkerSymbol"],
 //       function (Map, SimpleMarkerSymbol, GraphicsLayer,PictureMarkerSymbol) {
        function (Map, SimpleMarkerSymbol, GraphicsLayer) {
        mapApp.map = new Map("map", {
            basemap: "topo",  //For full list of pre-defined basemaps, navigate to http://arcg.is/1JVo6Wd
            center: [-58.3724715, -34.595986], // longitude, latitude
            zoom: 13
        });

        mapApp.POIsLayer = new GraphicsLayer({ id: "squares" });
            mapApp.map.addLayer(mapApp.POIsLayer);

            //Intenté añadir pictureMarkerSymbol de varias maneras pero no logré que corra el JS
            //mapApp.POIsSymbol = new PictureMarkerSymbol({
            //    "url": "MapsTest\bin\Data\icon.jpg",
            //    "height": 20,
            //    "width": 20,
            //    "type": "esriPMS"
            //});

        mapApp.POIsSymbol = new SimpleMarkerSymbol({
                "color": [120, 150, 0, 150],
                "size": 12,
                "angle": 45,
                "xoffset": 0,
                "yoffset": 0,
                "type": "esriSMS",
                "style": "esriSMSSquare",
                "outline": {
                    "color": [0, 0, 0, 255],
                    "width": 1,
                    "type": "esriSLS",
                    "style": "esriSLSSolid"
                }
            });
        
        if (callback) callback();
    });
}

mapApp.initMap(function () {
    mapApp.getPOIs(function (pois) {
        //console.log(pois);
        require(["esri/graphic", "esri/geometry/Point", "esri/geometry/webMercatorUtils", "dojo/domReady!"],
            function (Graphic, Point, webMercatorUtils) {
                pois.forEach(function (poiData) {
                    var coors = webMercatorUtils.lngLatToXY(poiData.XLon, poiData.YLat,)
                    var poi = new Point(coors[0], coors[1], mapApp.map.spatialReference)
                    mapApp.POIsLayer.add(new Graphic(poi, mapApp.POIsSymbol, poiData));
                })
            });
    })

    mapApp.getCats(function (categories) {
        var select = document.getElementById('category');
        //Se llaman a las categorias para añadirlas al dropdown
        $.each(categories, function (index) {
            var option = document.createElement("option");
            option.text = this.Value;
            option.value = this.Value;
            select.appendChild(option);
        });
    });
});
