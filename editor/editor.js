var tileSize = 16;
var tilemap = document.querySelector("#tilemap");
var tileset = document.querySelector("#tileset");
var setctx, mapctx;
// Virtual
var mapcanvas = document.createElement("canvas");
var setimage = null;
var mapimage = null;
var currentTile = [0,0];
var mapname = "tilemap";

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
    // First draw onto virtual
    mapcanvas.width = image.width;
    mapcanvas.height = image.height;
    var ctx = mapcanvas.getContext("2d");
    ctx.clearRect(0, 0, mapcanvas.width, mapcanvas.height);
    ctx.drawImage(image);
    mapimage = ctx.getImageData(0, 0, mapcanvas.width, mapcanvas.height);
    // Then redraw using mapimage if we have a tileset
    if(setimage != null)
        drawTilemap();
    else
        clearTilemap();
};

var createTilemap = function(w, h){
    mapcanvas.width = w;
    mapcanvas.height = h;
    var ctx = mapcanvas.getContext("2d");
    ctx.clearRect(0, 0, mapcanvas.width, mapcanvas.height);
    mapimage = ctx.getImageData(0, 0, mapcanvas.width, mapcanvas.height);
    drawTilemap();
};

var getTilemap = function(){
    mapcanvas.getContext("2d").putImageData(mapimage, 0, 0);
    return mapcanvas.toDataURL("image/png");
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

var drawTileset = function(){
    var tileW = setimage.width / tileSize;
    var tileH = setimage.height / tileSize;
    var tilesPerRow = Math.floor(tileset.clientWidth/tileSize);
    tileset.height = Math.ceil(tileW*tileH/tilesPerRow)*tileSize;
    clearTileset();
    var tx = 0;
    var ty = 0;
    for(var iy=0; iy<setimage.width; iy+=tileSize){
        for(var ix=0; ix<setimage.height; ix+=tileSize){
            setctx.drawImage(setimage, ix, iy, tileSize, tileSize,
                             tx, ty, tileSize, tileSize);
            tx += tileSize;
            if(tileset.width <= tx){
                tx = 0;
                ty += tileSize;
            }
        }
    }
};

var useTileset = function(image){
    var tileW = image.width / tileSize;
    var tileH = image.height / tileSize;
    console.log("Using new tileset (",tileW,"x",tileH,")");
    setimage = image;

    drawTileset();
    // Refresh map
    if(mapimage != null)
        drawTilemap();
};

var selectTile = function(x, y){
    var i;
    var tpr = Math.floor(setimage.width/tileSize);
    if(y === null){
        i = x+currentTile[0]+currentTile[1]*tpr;
    }else{
        i = x+y*Math.floor(tileset.width/tileSize);
    }
    currentTile = [Math.max(0, Math.min(setimage.width, i % tpr)),
                   Math.max(0, Math.min(setimage.height, Math.floor(i / tpr)))];
    console.log("Selected tile",currentTile);
};

var editMap = function(x, y, action){
    var pixelIndex = ((mapimage.width*y)+x)*4;
    if(action === "place"){
        mapimage.data[pixelIndex+0] = currentTile[0];
        mapimage.data[pixelIndex+1] = currentTile[1];
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

var initEvents = function(){
    document.querySelector("#new-tilemap").addEventListener("click", newTilemap);
    document.querySelector("#save-tilemap").addEventListener("click", saveTilemap);
    document.querySelector("#open-tilemap").addEventListener("click", openTilemap);
    document.querySelector("#open-tileset").addEventListener("click", openTileset);
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
    clearTileset();
};

var init = function(){
    console.log("Init.");
    initEvents();
    initCanvas();
};

init();
