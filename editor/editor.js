// COPYPAPSTA from Chunk.cs
var solids = {
    SolidPlatform:  0xFF000000, // A solid wall or platform
    HidingSpot:     0xFF404040, // A hiding spot for the player
    BackgroundWall: 0xFF808080, // A wall for enemies, but not the player
    FallThrough:    0xFFC0C0C0, // A jump/fall-through platform
    PlayerStart:    0xFF00FF00, // The start position for the player
    StaticCamera:   0xFFFF0100, // An enemy spawn position
    PivotCamera:    0xFFFF0200, // An enemy spawn position
    GroundDrone:    0xFFFF0300, // An enemy spawn position
    AerialDrone:    0xFFFF0400, // An enemy spawn position
    Spike:          0xFF0000FF, // A death spike
    Pickup:         0xFF00EEFF, // An information pickup item
    Goal:           0xFFFFEE00, // A goal tile leading to end-of-level
};

var tileSize = 16;
var tilemap = document.querySelector("#tilemap");
var tileset = document.querySelector("#tileset");
var overlay = document.querySelector("#overlay");
var setctx, mapctx;
// Virtual
var tempcanvas = document.createElement("canvas");
var setpixels = null, setimage = null;
var mapimage = null;
var nonNullTiles = [];
var currentTile = 0;
var mapname = "tilemap";

var pad = function(string, length, char){
    char = char || " ";
    string = string+"";
    while(string.length < length){
        string = char + string;
    }
    return string;
};

var getPixel = function(data, x, y){
    var index = (x+y*data.width)*4;
    var r = data.data[index+0];
    var g = data.data[index+1];
    var b = data.data[index+2];
    var a = data.data[index+3];
    return ((r << 24) + (g << 16) + (b << 8) + a) >>> 0;
};

var formatRGB = function(pixel){
    var string = pixel.toString(16);
    return "#"+("00000000" + string).slice(-8);
};

var getImagePixels = function(image, dim){
    dim = dim || [image.width, image.height];
    tempcanvas.width = dim[0];
    tempcanvas.height = dim[1];
    var ctx = tempcanvas.getContext("2d");
    ctx.clearRect(0, 0, tempcanvas.width, tempcanvas.height);
    if(image) ctx.drawImage(image, 0, 0);
    return ctx.getImageData(0, 0, tempcanvas.width, tempcanvas.height);
};

var getImage = function(imagedata){
    tempcanvas.width = imagedata.width;
    tempcanvas.height = imagedata.height;
    var ctx = tempcanvas.getContext("2d");
    ctx.putImageData(imagedata, 0, 0);
    var image = new Image();
    image.src = tempcanvas.toDataURL();
    return image;
};

var drawTilemapGrid = function(){
    for(var y=0; y<tilemap.height; y+=tileSize){
        mapctx.beginPath();
        mapctx.moveTo(0, y+0.5);
        mapctx.lineTo(tilemap.width, y+0.5);
        mapctx.stroke();
    }
    for(var x=0; x<tilemap.width; x+=tileSize){
        mapctx.beginPath();
        mapctx.moveTo(x+0.5, 0);
        mapctx.lineTo(x+0.5, tilemap.height);
        mapctx.stroke();
    }
};

var clearTilemap = function(){
    mapctx.fillStyle = "#FFFFFF";
    mapctx.strokeStyle = "#DDDDDD";
    mapctx.fillRect(0, 0, tilemap.width, tilemap.height);
    drawTilemapGrid();
};

var drawTilemapPos = function(x, y){
    var pixelIndex = ((mapimage.width*y)+x)*4;
    var r = mapimage.data[pixelIndex+0];
    var g = mapimage.data[pixelIndex+1];
    var a = mapimage.data[pixelIndex+3];
    if(0 < a){
        mapctx.drawImage(setimage, r*tileSize, g*tileSize, tileSize, tileSize,
                         x*tileSize, y*tileSize, tileSize, tileSize);
    }else{
        mapctx.fillRect(x*tileSize, y*tileSize, tileSize, tileSize);
        mapctx.strokeStyle = "#DDDDDD";
        mapctx.beginPath();
        mapctx.moveTo(x*tileSize+0.5, (y+1)*tileSize-0.5);
        mapctx.lineTo(x*tileSize+0.5, y*tileSize+0.5);
        mapctx.lineTo((x+1)*tileSize-0.5, y*tileSize+0.5);
        mapctx.stroke();
    }
};

var drawTilemap = function(){
    tilemap.width = mapimage.width*tileSize;
    tilemap.height = mapimage.height*tileSize;
    overlay.width = tilemap.width;
    overlay.height = tilemap.height;
    mapctx.fillStyle = "#FFFFFF";
    mapctx.fillRect(0, 0, tilemap.width, tilemap.height);
    for(var y=0; y<mapimage.height; y+=1){
        for(var x=0; x<mapimage.width; x+=1){
            drawTilemapPos(x, y);
        }
    }
};

var useTilemap = function(image){
    console.log("Using new tilemap (",image.width,"x",image.width,")");
    mapimage = getImagePixels(image);
    // Then redraw using mapimage if we have a tileset
    if(setpixels != null)
        drawTilemap();
    else
        clearTilemap();
};

var createTilemap = function(w, h){
    mapimage = getImagePixels(null, [w, h]);
    drawTilemap();
};

var getTilemap = function(){
    tempcanvas.getContext("2d").putImageData(mapimage, 0, 0);
    return tempcanvas.toDataURL("image/png");
};

var clearTileset = function(){
    var s = tileSize/2;
    tileset.width = tileset.clientWidth;
    setctx.fillStyle = "#808080";
    setctx.fillRect(0, 0, tileset.width, tileset.height);
    setctx.fillStyle = "#C0C0C0";
    for (var y=0; y<tileset.height; y+=s) {
        for (var x=0; x<tileset.width; x+=s) {
            if(((y+x)/s)%2 == 0)
                setctx.rect(x, y, s, s);
        }
    }
    setctx.fill();
};

var isTileEmpty = function(data, x, y){
    for(var iy=y*tileSize; iy<(y+1)*tileSize; iy++){
        for(var ix=x*tileSize; ix<(x+1)*tileSize; ix++){
            // Read alpha component
            if(data.data[(ix+iy*data.width)*4+3] != 0)
                return false;
        }
    }
    return true;
};

var drawTileset = function(){
    var tileW = setimage.width / tileSize;
    var tileH = setimage.height / tileSize;
    var tilesPerRow = Math.floor(tileset.clientWidth/tileSize);
    tileset.height = Math.ceil(tileW*tileH/tilesPerRow)*tileSize;
    clearTileset();
    nonNullTiles = [];
    var tx = 0, ty = 0;
    for(var iy=0; iy<tileH; iy++){
        for(var ix=0; ix<tileW; ix++){
            // Check empty.
            if(!isTileEmpty(setpixels, ix, iy)){
                setctx.drawImage(setimage, ix*tileSize, iy*tileSize, tileSize, tileSize,
                                 tx*tileSize, ty*tileSize, tileSize, tileSize);
                nonNullTiles[tx+ty*tilesPerRow] = [ix, iy];
                tx++;
                if(tilesPerRow <= tx){
                    tx = 0;
                    ty++;
                }
            }
        }
    }
};

var createSolidsPixels = function(){
    var data = new ImageData(tileSize*256, tileSize*256);
    for(var key in solids){
        var color = solids[key];
        var r = (color & 0x000000FF) >>>  0;
        var g = (color & 0x0000FF00) >>>  8;
        var b = (color & 0x00FF0000) >>> 16;
        var a = (color & 0xFF000000) >>> 24;
        for(var y=g*tileSize; y<(g+1)*tileSize; y++){
            for(var x=r*tileSize; x<(r+1)*tileSize; x++){
                var index = (x+y*data.width)*4;
                data.data[index+0] = r;
                data.data[index+1] = g;
                data.data[index+2] = b;
                data.data[index+3] = a;
            }
        }
    }
    return data;
};

var selectTile = function(x, y){
    if(y === null){
        currentTile += x;
    }else{
        currentTile = x+y*Math.floor(tileset.width/tileSize);
    }
    currentTile = (currentTile<0)? 0 : (nonNullTiles.length <= currentTile)? nonNullTiles.length-1 : currentTile;

    var tile = nonNullTiles[currentTile];
    var format = formatRGB(getPixel(setpixels, tile[0]*tileSize, tile[1]*tileSize));
    document.querySelector("#selected").innerText = pad(tile[0], 3)+","+pad(tile[1], 3);
    document.querySelector("#color").innerText = format;
    console.log("Selected tile", currentTile, tile, format);
};

var useTileset = function(image){
    var tileW = image.width / tileSize;
    var tileH = image.height / tileSize;
    console.log("Using new tileset (",tileW,"x",tileH,")");
    if(image instanceof Image){
        setpixels = getImagePixels(image);
        setimage = image;
    }else if(image instanceof ImageData){
        setpixels = image;
        setimage = getImage(image);
    }else{
        throw new Error("Wtf?");
    }

    drawTileset();
    // Refresh map
    if(mapimage != null)
        drawTilemap();
    selectTile(0, 0);
};

var editMap = function(x, y, action){
    var pixelIndex = ((mapimage.width*y)+x)*4;
    if(action === "place"){
        var tile = nonNullTiles[currentTile];
        mapimage.data[pixelIndex+0] = tile[0];
        mapimage.data[pixelIndex+1] = tile[1];
        mapimage.data[pixelIndex+3] = 255;
    }else if(action === "erase"){
        mapimage.data[pixelIndex+0] = 0;
        mapimage.data[pixelIndex+1] = 0;
        mapimage.data[pixelIndex+3] = 0;
    }
    drawTilemapPos(x,y);
    console.log("Edited (",x,"x",y,")");
};

var selectTileEvent = function(ev){
    if(ev instanceof WheelEvent){
        selectTile(-Math.sign(ev.deltaY), null);
    }else if(ev instanceof MouseEvent){
        var x = Math.floor(ev.offsetX/tileSize);
        var y = Math.floor(ev.offsetY/tileSize);
        selectTile(x, y);
    }
};

var button = 0;
var editMapEvent = function(ev){
    if(ev.buttons){
        var x = Math.floor(ev.offsetX/tileSize);
        var y = Math.floor(ev.offsetY/tileSize);
        var action = (button == 2)? "erase" : "place";
        editMap(x, y, action);
    }
};

var newTilemap = function(){
    var prompt = document.querySelector("#new-prompt");
    prompt.style.display = "block";
    prompt.querySelector("#new-ok").onclick = function(){
        prompt.style.display = "none";
        mapname = prompt.querySelector("#new-name").value;
        var w = parseInt(prompt.querySelector("#new-width").value);
        var h = parseInt(prompt.querySelector("#new-height").value);
        createTilemap(w, h);
    };
};

var useOverlay = function(image){
    overlay.src = image.src;
    if(!mapimage || mapimage.width != image.width || mapimage.height != image.height)
        createTilemap(image.width, image.height);
};

var loadFileCallback;
var loadFile = function(ev){
    var f = ev.target.files[0];
    var fr = new FileReader();

    fr.onload = function(){
        loadFileCallback(fr.result);
    };
    fr.readAsDataURL(f);
};

var openImage = function(callback){
    loadFileCallback = function(data){
        var image = new Image;
        image.onload = function(){
            callback(image);
        };
        image.src = data;
    };
    document.querySelector("nav>input[type=file]").click();
};

var saveTilemap = function(){
    var data = getTilemap();
    var link = document.createElement("a");
    link.setAttribute("download", mapname+".png");
    link.setAttribute("href", data.replace("image/png", "image/octet-stream"));
    link.click();
};

var openTilemap = function(){
    openImage(useTilemap);
};

var openTileset = function(){
    openImage(useTileset);
};

var openOverlay = function(){
    openImage(useOverlay);
};

var initEvents = function(){
    document.querySelector("#new-tilemap").addEventListener("click", newTilemap);
    document.querySelector("#save-tilemap").addEventListener("click", saveTilemap);
    document.querySelector("#open-tilemap").addEventListener("click", openTilemap);
    document.querySelector("#open-tileset").addEventListener("click", openTileset);
    document.querySelector("#open-overlay").addEventListener("click", openOverlay);
    document.querySelector("nav>input[type=file]").addEventListener("change",loadFile);
    window.addEventListener("wheel", selectTileEvent);
    window.addEventListener("resize", function(){drawTileset();});
    tileset.addEventListener("click", selectTileEvent);
    tilemap.addEventListener("mousedown", function(ev){button = ev.button; editMapEvent(ev);});
    tilemap.addEventListener("mousemove", editMapEvent);
    tilemap.addEventListener("contextmenu", function(ev){ev.preventDefault();});
};

var initCanvas = function(){
    mapctx = tilemap.getContext("2d");
    mapctx.mozImageSmoothingEnabled = false;
    mapctx.webkitImageSmoothingEnabled = false;
    mapctx.msImageSmoothingEnabled = false;
    mapctx.imageSmoothingEnabled = false;
    clearTilemap();
    
    setctx = tileset.getContext("2d");
    setctx.mozImageSmoothingEnabled = false;
    setctx.webkitImageSmoothingEnabled = false;
    setctx.msImageSmoothingEnabled = false;
    setctx.imageSmoothingEnabled = false;
    useTileset(createSolidsPixels());
};

var init = function(){
    console.log("Init.");
    initEvents();
    initCanvas();
};

init();
