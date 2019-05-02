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
    Door:           0xFF00337F, // A door which can be opened and closed
    DialogTrigger   0xFFDC00FF, // Triggers a dialog like a pickup.
	
	ClimbPrompt:    0xFF0199AA,
    CrouchPrompt:   0xFF0299AA,
    JumpPrompt:     0xFF0399AA,
    UpRightPrompt:  0xFF0499AA,
    DownPrompt:     0xFF0599AA
};

const tileSize = 16;
const defaultLayers = 5;
const layerNames = ["Solids", "Background", "Decals", "Ground", "Foreground", "Special"];
const minSize = [40,26];
var tilemap = document.querySelector("#tilemap");
var tilelist = document.querySelector("#tilelist");
var levelmap = document.querySelector("#levelmap");
var listctx, mapctx;
// Virtual
var tempcanvas = document.createElement("canvas");
var solidset = null;
var level = null;
var zoom = 1.0;

/// Helper functions
var parseStory = function(string){
    var items = string.split("\n---\n");
    for(var i in items){
        items[i] = items[i].split("\n\n");
    }
    return items;
};

var printStory = function(story){
    var items = [];
    for(var item of story){
        items.push(item.join("\n\n"));
    }
    return items.join("\n---\n");
};

var pad = function(string, length, char){
    char = char || " ";
    string = string+"";
    while(string.length < length){
        string = char + string;
    }
    return string;
};

var arrayToPixel = function(arr, offset){
    offset = offset || 0;
    var r = arr[offset+0];
    var g = arr[offset+1];
    var b = arr[offset+2];
    var a = arr[offset+3] || 255;
    return ((r << 24) + (g << 16) + (b << 8) + a) >>> 0;
};

var getPixel = function(data, x, y){
    return arrayToPixel(data.data, (x+y*data.width)*4);
};

var formatRGB = function(pixel){
    var string = pixel.toString(16);
    return "#"+("00000000" + string).slice(-8);
};

var abgrToRgba = function(abgr){
    return(((abgr & 0x000000FF) << 24) >>>  0)
        + (((abgr & 0x0000FF00) <<  8) >>>  0)
        + (((abgr & 0x00FF0000) <<  0) >>>  8)
        + (((abgr & 0xFF000000) <<  0) >>> 24);
};

var tileType = function(pixel){
    for(var solid in solids){
        if(abgrToRgba(solids[solid]) === pixel)
            return solid;
    }
    return null;
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
    
    return new Promise(function(accept){
        image.onload = function(){accept(image);};
        image.src = tempcanvas.toDataURL();
    });
};

var getImageBase64 = function(image){
    tempcanvas.width = image.width;
    tempcanvas.height = image.height;
    var ctx = tempcanvas.getContext("2d");
    ctx.clearRect(0, 0, tempcanvas.width, tempcanvas.height);
    if(image instanceof ImageData)
        ctx.putImageData(image, 0, 0);
    else
        ctx.drawImage(image, 0, 0);
    return tempcanvas.toDataURL("image/png").replace(/.*?base64,/, "");
};

var fillCheckerboard = function(canvas, ctx){
    var s = tileSize/2;
    ctx = ctx || canvas.getContext("2d");
    ctx.fillStyle = "#808080";
    ctx.fillRect(0, 0, canvas.width, canvas.height);
    ctx.fillStyle = "#C0C0C0";
    for (var y=0; y<canvas.height; y+=s) {
        for (var x=0; x<canvas.width; x+=s) {
            if(((y+x)/s)%2 == 0)
                ctx.rect(x, y, s, s);
        }
    }
    ctx.fill();
    return ctx;
};

var resizePixels = function(pixels, w, h){
    tempcanvas.width = w;
    tempcanvas.height = h;
    var ctx = tempcanvas.getContext("2d");
    // Deposit in lower left corner.
    ctx.putImageData(pixels, 0, tempcanvas.height - pixels.height);
    return ctx.getImageData(0, 0, tempcanvas.width, tempcanvas.height);
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

var loadFile = function(input, type){
    return new Promise(function(accept, reject){
        if(input.files.length == 0) reject("No file selected.");
        
        var f = input.files[0];
        var fr = new FileReader();
        
        fr.onload = function(){
            accept([fr.result, f.name]);
        };
        fr.onabort = ()=>{reject("File loading got aborted.");};
        fr.onerror = ()=>{reject("File loading failed.");};

        console.log("Loading",f,"as",type);
        switch(type){
        case "data": fr.readAsDataURL(f); break;
        case "text": fr.readAsText(f); break;
        case "binary": fr.readAsBinaryString(f); break;
        case "array": fr.readAsArrayBuffer(f); break;
        default: throw new Error("Invalid data type: "+type);
        }
    });
};

var loadImage = function(data, name){
    if(data.constructor === Array){
        name = data[1];
        data = data[0];
    }
    var image = new Image();
    return new Promise(function(accept){
        image.onload = function(){
            accept(image);
        };
        image.src = data;
        image.name = name;
    });
};

var loadTileset = function(input){
    var name = input.files[0].name;
    name = name.substr(0, name.lastIndexOf("."));
    return loadFile(input, "data")
        .then(loadImage, (e)=>{showPrompt(".prompt.error", e); return null;})
        .then(image => new Tileset({image: image, name: name}));
};

var openFile = function(type){
    var input = document.createElement("input");
    input.type = "file";
    input.accept = type;

    return new Promise(function(accept){
        input.addEventListener("change", function(){
            accept(input);
        });
        input.click();
    });
};

var saveFile = function(data, filename){
    var link = document.createElement("a");
    data = data.replace(/data:.*?;/, "data:application/octet-stream;");
    link.setAttribute("download", filename || "file.dat");
    link.setAttribute("href", data);
    link.setAttribute("style", "display:none");
    document.body.appendChild(link);
    link.click();
    document.body.removeChild(link);
};

var constructElement = function(tag, options){
    var el = document.createElement(tag);
    el.setAttribute("class", (options.classes||[]).join(" "));
    if(options.id) el.setAttribute("id", options.id);
    if(options.text) el.innerText = options.text;
    if(options.html) el.innerHTML = options.html;
    for(let attr in (options.attributes||{})){
        el.setAttribute(attr, options.attributes[attr]);
    }
    for(let data in (options.data||{})){
        el.dataset[data] = options.data[data];
    }
    for(let attr in (options.style||{})){
        el.style[attr] = options.style[attr];
    }
    for(let tag in (options.elements||{})){
        let content = options.elements[tag];
        let sub = self.constructElement(content.tag || tag, content);
        el.appendChild(sub);
    }
    return el;
};

/// Base classes
class Tileset{
    constructor(init){
        init = init || {};
        this.image = null;
        this.pixels = null;
        this.name = init.name || "?";
        this.rgMap = init.rgMap || {};
        this.tileMap = init.tileMap || [];
        this.currentTile = [0, 0];
        this.tilesPerRow = 32;

        if(this.tileMap.length == 0 && (init.image || init.pixels))
            this.preprocess(init.image, init.pixels);
        else{
            this.image = init.image;
            this.pixels = init.pixels;
        }
    }

    preprocess(image, pixels, compress){
        if(!pixels)
            pixels = getImagePixels(image);
        if(!image){
            return getImage(pixels)
                .then((image)=>this.preprocess(image, pixels));
        }
        // First compute maps
        var tileW = pixels.width / tileSize;
        var tileH = pixels.height / tileSize;
        this.tilesPerRow = tileW;
        this.tileMap[0] = new Array(this.tilesPerRow);
        var tx=0, ty=0;
        for(var iy=0; iy<tileH; iy++){
            for(var ix=0; ix<tileW; ix++){
                if(!isTileEmpty(pixels, ix, iy) || !compress){
                    this.tileMap[ty][tx] = [ix, iy, 0];
                    this.rgMap[(ix<<8) + iy] = [tx, ty];
                    tx++;
                    if(this.tilesPerRow <= tx){
                        tx = 0;
                        ty++;
                        this.tileMap[ty] = new Array(this.tilesPerRow);
                    }
                }
            }
        }
        // Then compute static, compressed image
        tempcanvas.width = this.tileMap[0].length*tileSize;
        tempcanvas.height = this.tileMap.length*tileSize;
        var ctx = tempcanvas.getContext("2d");
        ctx.clearRect(0, 0, tempcanvas.width, tempcanvas.height);
        for(var iy=0; iy<this.tileMap.length; iy++){
            for(var ix=0; ix<this.tileMap[iy].length; ix++){
                var rg = this.tileMap[iy][ix];
                if(rg){
                    ctx.drawImage(image, rg[0]*tileSize, rg[1]*tileSize, tileSize, tileSize,
                                  ix*tileSize, iy*tileSize, tileSize, tileSize);
                }
            }
        }
        this.pixels = ctx.getImageData(0, 0, tempcanvas.width, tempcanvas.height);
        this.image = new Image();
        this.image.onload = ()=>{this.use(true);};
        this.image.src = tempcanvas.toDataURL();
        return this;
    }

    get selected(){ return this.currentTile; }
    set selected(value){
        var x = value[0];
        var y = value[1];
        y = (y<0)? 0 : (this.tileMap.length <= y) ? this.tileMap.length-1 : y;
        x = (x<0)? 0 : (this.tileMap[y].length <= x) ? this.tileMap[y].length-1 : x;
        this.currentTile = [x, y];

        var tile = this.tileMap[y][x];
        var pixel = getPixel(this.pixels, x*tileSize, y*tileSize);
        document.querySelector("#selected").innerText = pad(tile[0], 3)+","+pad(tile[1], 3);
        document.querySelector("#color").innerText = formatRGB(pixel);
        document.querySelector("#type").innerText = tileType(pixel);
        return this.currentTile;
    }

    show(){
        console.log("Showing", this);
        tilelist.width = this.tileMap[0].length*tileSize;
        tilelist.height = this.tileMap.length*tileSize;
        fillCheckerboard(tilelist);
        listctx.drawImage(this.image, 0, 0);
        return this;
    }

    use(force){
        if(level.chunk.tileset != this || force){
            console.log("Using", this);
            level.chunk.tileset = this;
            level.chunk.show();
            this.show();
        }
        return this;
    }
}

class Chunk{
    constructor(init){
        init = init || {};
        this.name = init.name || "chunk";
        this.position = init.position || [0, 0];
        this.currentLayer = 0;
        this.outside = init.outside || false;
        this.background = init.background || new Image();
        this.tileset = init.tileset || level.defaultTileset;
        this.storyItems = init.storyItems || [];
        this.pixels = init.pixels;
        
        if(!this.pixels){
            var layers = init.layers || defaultLayers;
            this.pixels = new Array(layers);
            for(var i=0; i<layers; i++){
                this.pixels[i] = new ImageData(init.width || minSize[0], init.height || minSize[1]);
            }
        }
        for(var i=0; i<this.pixels.length; i++){
            this.preprocess(this.pixels[i], i);
        }
    }

    preprocess(pix, i){
        let self = this;
        let pixels = pix;
        pixels.id = i;
        pixels.visible = true;
        pixels.opacity = (i == 0)? 0.7 : 1.0;
        pixels.show = function(){
            pixels.visible = true;
            var layer = self.uiElement.querySelector(".layer[data-id=\""+pixels.id+"\"]");
            layer.querySelector(".show").style.display = "none";
            layer.querySelector(".hide").style.display = "block";
            self.show();
        };
        pixels.hide = function(){
            pixels.visible = false;
            var layer = self.uiElement.querySelector(".layer[data-id=\""+pixels.id+"\"]");
            layer.querySelector(".show").style.display = "block";
            layer.querySelector(".hide").style.display = "none";
            self.show();
        };
        pixels.use = function(){
            self.layer = pixels.id;
        };
        return pixels;
    }

    get index(){ return level.chunks.indexOf(this); }
    get uiElement(){ return level.uiElement.querySelectorAll(".chunk")[this.index]; }
    get width(){ return this.pixels[0].width; }
    get height(){ return this.pixels[0].height; }
    get layers(){ return this.pixels.length; }
    get layer(){ return this.pixels[this.currentLayer]; }
    set layer(layer){
        var index = null;
        if(layer instanceof ImageData)
            index = this.pixels.indexOf(layer);
        if(Number.isInteger(layer))
            index = layer;
        index = (index < 0)? 0 :(this.layers <= index)? this.layers-1 :index;
        
        [].forEach.call(this.uiElement.querySelectorAll(".layer"), (e, i)=>{
            if(i == index) e.classList.add("selected");
            else           e.classList.remove("selected");});
        level.uiElement.querySelector("#opacity").value = this.pixels[index].opacity;
        
        this.currentLayer = index;
        if(level.chunk != this) this.use();
        else                    this.getTileset().show();
        return this.currentLayer;
    }

    getLayer(layer){
        if(layer === undefined)layer = this.currentLayer;
        return this.pixels[layer];
    }
    
    getTileset(layer){
        if(layer === undefined)layer = this.currentLayer;
        return (0 < layer)? this.tileset: solidset;
    }

    drawPos(x, y){
        var pixelIndex = ((this.width*y)+x)*4;
        // Clear and fill background pattern
        mapctx.clearRect(x*tileSize, y*tileSize, tileSize, tileSize);
        mapctx.strokeStyle = "#DDDDDD";
        mapctx.beginPath();
        mapctx.moveTo(x*tileSize+0.5, (y+1)*tileSize-0.5);
        mapctx.lineTo(x*tileSize+0.5, y*tileSize+0.5);
        mapctx.lineTo((x+1)*tileSize-0.5, y*tileSize+0.5);
        mapctx.stroke();
        // Draw actual tile.
        for(var l of [1, 2, 3, 4, 0]){
            var pixels = this.pixels[l];
            if(pixels.visible){
                var r = pixels.data[pixelIndex+0];
                var g = pixels.data[pixelIndex+1];
                var a = pixels.data[pixelIndex+3];
                if(0 < a){
                    var tileset = this.getTileset(l);
                    var s = tileset.rgMap[(r<<8) + g];
                    if(!s) break;
                    mapctx.globalAlpha = pixels.opacity;
                    mapctx.drawImage(tileset.image,
                                     s[0]*tileSize, s[1]*tileSize, tileSize, tileSize,
                                     x*tileSize, y*tileSize, tileSize, tileSize);
                }
            }
        }
        mapctx.globalAlpha = 1.0;
        return this;
    }

    resize(width, height){
        for(var l=0; l<this.pixels.length; l++){
            this.pixels[l] = this.preprocess(resizePixels(this.pixels[l], width, height), l);
        }
        // FIXME: Kludge
        generateSidebar(level);
        this.show();
        level.zoom(zoom);
        return this;
    }

    clear(){
        console.log("Clearing", this);
        tilemap.width = this.width*tileSize;
        tilemap.height = this.height*tileSize;
        mapctx.fillStyle = "#FFFFFF";
        mapctx.strokeStyle = "#DDDDDD";
        mapctx.fillRect(0, 0, tilemap.width, tilemap.height);
        // FIXME: clear pixels
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
        return this;
    }

    show(){
        console.log("Showing", this);
        tilemap.width = this.width*tileSize;
        tilemap.height = this.height*tileSize;
        mapctx.fillStyle = "#FFFFFF";
        mapctx.fillRect(0, 0, tilemap.width, tilemap.height);
        for(var y=0; y<this.height; y+=1){
            for(var x=0; x<this.width; x+=1){
                this.drawPos(x, y);
            }
        }
        level.zoom(zoom);
        return this;
    }

    edit(x, y, action, layer){
        if(layer === undefined)layer = this.currentLayer;
        var pixels = this.pixels[layer];
        var pi = (x, y)=>((pixels.width*y)+x)*4;
        var p = (x, y)=>{
            let i = pi(x,y);
            return arrayToPixel(pixels.data, i);
        };
        var sp = (x, y, tile)=>{
            let i = pi(x,y);
            pixels.data[i+0] = tile[0];
            pixels.data[i+1] = tile[1];
            pixels.data[i+2] = tile[2];
            pixels.data[i+3] = (tile[3] === undefined)? 255 : tile[3];
        };
        if(action === "place"){
            let tileset = this.getTileset(layer);
            sp(x, y, tileset.tileMap[tileset.selected[1]][tileset.selected[0]]);
            this.drawPos(x,y);
        }else if(action === "erase"){
            sp(x, y, [0,0,0,0]);
            this.drawPos(x,y);
        }else if(action === "fill"){
            let tileset = this.getTileset(layer);
            var fill = tileset.tileMap[tileset.selected[1]][tileset.selected[0]];
            var queue = [[x,y]];
            var find = p(x,y);
            var width = pixels.width, height = pixels.height;
            if(find === arrayToPixel(fill))
                return this;
            while(0<queue.length){
                let c = queue.pop();
                let w = c[0], e = c[0];
                let y = c[1];
                while(0 < w && p(w-1, y) === find)
                    w--;
                while(e < width-1 && p(e+1, y) === find)
                    e++;
                for(let i=w; i<=e; i++){
                    sp(i, y, fill);
                    if(y<height-1 && p(i, y+1) === find)
                        queue.push([i, y+1]);
                    if(0<y && p(i, y-1) === find)
                        queue.push([i, y-1]);
                }
            }
            this.show();
        }
        //console.log("Edited",this,":",layer,"(",x,"x",y,")");
        return this;
    }

    serialize(){
        var layers = new Array(this.layers);
        for(var i=0; i<layers.length; i++)
            layers[i] = "chunks/"+this.name+"-"+i+".png";
        return {
            name: this.name,
            position: [ this.position[0]*tileSize, this.position[1]*tileSize ],
            layers: layers,
            tileset: this.tileset.name,
            outside: this.outside,
            storyItems: this.storyItems,
            background: this.background.name,
        };
    }

    delete(){
        console.log("Deleting", this);
        this.uiElement.parentNode.removeChild(this.uiElement);
        var index = level.chunks.indexOf(this);
        level.chunks.splice(index, 1);
        level.chunk = index;
    }

    use(force){
        if(level.chunk != this || force){
            console.log("Using", this);
            level.chunk = this;
            this.getTileset().show();
            this.layer = this.currentLayer;
            return this.show();
        }
        return this;
    }
}

class Level{
    constructor(init){
        init = init || {};
        this.name = init.name || "level";
        this.description = init.description || "";
        this.preview = init.preview || null;
        this.next = init.next || null;
        this.startChase = init.startChase || false;
        this.startChunk = init.startChunk || 0;
        this.defaultTileset = init.defaultTileset || solidset;
        this.chunks = init.chunks || [ new Chunk({tileset: this.defaultTileset}) ];
        this.currentChunk = 0;
    }

    get uiElement(){ return document.querySelector(".sidebar"); }
    get chunk(){ return this.chunks[this.currentChunk]; }
    set chunk(chunk) {
        var index = null;
        if(chunk instanceof Chunk)
            index = this.chunks.indexOf(chunk);
        if(typeof chunk === 'string' || chunk instanceof String)
            this.chunks.forEach((c,i) => {if(c.name == chunk) index = i;});
        if(Number.isInteger(chunk))
            index = chunk;
        index = (index < 0)? 0 : (this.chunks.length <= index)? this.chunks.length-1 : index;
        
        [].forEach.call(this.uiElement.querySelectorAll(".chunk"), (e, i)=>{
            if(i == index) e.classList.add("selected");
            else           e.classList.remove("selected");});
        this.currentChunk = index;
        return this.chunk.show();
    }
    
    zoom(factor){
        var ui = document.querySelector("#zoom");
        ui.parentNode.querySelector("label").innerText = factor.toFixed(1);
        ui.value = factor;
        tilemap.style.maxWidth = tilemap.style.minWidth = (tilemap.width*factor)+"px";
        tilemap.style.maxHeight = tilemap.style.minHeight = (tilemap.height*factor)+"px";
        zoom = factor;
        generateLevelmap(level);
    }

    serialize(){
        var chunks = [];
        for(var i=0; i<this.chunks.length; i++){
            chunks.push(this.chunks[i].serialize());
        }
        return {
            name: this.name,
            description: this.description,
            preview: (this.preview)? "preview.png" : null,
            next: this.next,
            startChase: this.startChase,
            startChunk: this.startChunk,
            chunks: chunks
        };
    }

    use(force){
        if(level != this || force){
            console.log("Using", this);
            level = this;
            generateSidebar(this);
            generateLevelmap(this);
            this.chunk.use(true);
        }
        return this;
    }
}

/// UI
var generateSidebar = function(level){
    var ui = level.uiElement;
    ui.innerHTML = "";
    var entry = constructElement("header",{
        elements: {
            label: {text: level.name},
            a1: {tag: "a", classes: ["change"], elements: {i: {classes: ["fas", "fa-fw", "fa-pen"]}}},
            a2: {tag: "a", classes: ["create"], elements: {i: {classes: ["fas", "fa-fw", "fa-plus"]}}}
        }
    });
    entry.querySelector("label").addEventListener("click", function(){toggleLevel(level);});
    entry.querySelector(".change").addEventListener("click", function(){editLevel(level);});
    entry.querySelector(".create").addEventListener("click", function(){newChunk(level);});

    var opacity = constructElement("input",{
        id: "opacity",
        attributes: {type: "range",min: 0.0, max: 1.0, step: 0.1}
    });
    opacity.addEventListener("change", function(){
        level.chunk.layer.opacity = parseFloat(opacity.value);
        level.chunk.show();
    });
    
    var list = constructElement("ul",{id: "chunks"});
    for(let chunk of level.chunks){
        let entry = constructElement("li",{
            classes: ["chunk", (chunk == level.chunk)? "selected": ""],
            data: {id: chunk.index},
            elements: {
                header: {elements: {
                    label: {text: chunk.name},
                    a1: {tag: "a", classes: ["change"], elements: {i: {classes: ["fas", "fa-fw", "fa-pen"]}}},
                    a2: {tag: "a", classes: ["delete"], elements: {i: {classes: ["fas", "fa-fw", "fa-trash"]}}}
                }},
                ul: {classes: ["layers"]}
            }
        });
        entry.querySelector("label").addEventListener("click", function(){chunk.use();});
        entry.querySelector(".change").addEventListener("click", function(){editChunk(chunk);});
        entry.querySelector(".delete").addEventListener("click", function(){chunk.delete();});
        
        var layers = entry.querySelector(".layers");
        for(let layer of chunk.pixels){
            let entry = constructElement("li", {
                classes: ["layer", (chunk == level.chunk && layer == chunk.layer)? "selected" : ""],
                data: {id: layer.id},
                elements: {
                    label: {text: layerNames[layer.id] || layer.id+""},
                    a1: {tag: "a", classes: ["show"], elements: {i: {classes: ["fas", "fa-fw", "fa-eye-slash"]}}},
                    a2: {tag: "a", classes: ["hide"], elements: {i: {classes: ["fas", "fa-fw", "fa-eye"]}}}
                }
            });
            if(layer.visible)
                entry.querySelector(".show").style.display = "none";
            else
                entry.querySelector(".hide").style.display = "none";
            entry.querySelector("label").addEventListener("click", function(){layer.use();});
            entry.querySelector(".show").addEventListener("click", function(){layer.show();});
            entry.querySelector(".hide").addEventListener("click", function(){layer.hide();});
            layers.appendChild(entry);
        }    
        list.appendChild(entry);
    }
    ui.appendChild(entry);
    ui.appendChild(opacity);
    ui.appendChild(list);
};

var generateLevelmap = function(level){
    var ui = levelmap;
    ui.innerHTML = "";

    var drag = null;
    var evPos = (ev)=>{
        var bounds = ui.getBoundingClientRect();
        return [ ev.clientX - bounds.left, ev.clientY - bounds.top ];
    };

    var updatePosition = (entry, chunk)=>{
        entry.style.left = ui.clientWidth/2 + (chunk.position[0]-chunk.width/2)*zoom + "px";
        entry.style.bottom = ui.clientHeight/2 + (chunk.position[1]-chunk.height/2)*zoom + "px";
    };
    
    for(let chunk of level.chunks){
        let entry = constructElement("canvas",{
            classes: ["chunk"],
            data: {id: chunk.index},
            attributes: {
                width: chunk.width,
                height: chunk.height,
                title: chunk.name
            },
            style: {
                minWidth: chunk.width*zoom + "px",
                minHeight: chunk.height*zoom + "px"
            }
        });
        var ctx = entry.getContext("2d");
        ctx.putImageData(chunk.getLayer(0), 0, 0);

        entry.addEventListener("mousedown", function(ev){
            drag = {mouse: evPos(ev),
                    chunk: chunk,
                    position: [ chunk.position[0], chunk.position[1] ],
                    entry: entry};
        });

        updatePosition(entry, chunk);
        ui.appendChild(entry);
    }
    
    ui.addEventListener("mousemove", function(ev){
        if(drag){
            var newPos = evPos(ev);
            var dPos = [ (newPos[0]-drag.mouse[0])/zoom, (drag.mouse[1]-newPos[1])/zoom ];
            drag.chunk.position[0] = drag.position[0] + Math.round(dPos[0]);
            drag.chunk.position[1] = drag.position[1] + Math.round(dPos[1]);
            updatePosition(drag.entry, drag.chunk);
        }
    });
    
    ui.addEventListener("mouseup", function(ev){
        if(drag){
            // Push out of other blocks repeatedly until we no longer get collisions.
            var found = true;
            while(found){
                found = false;
                for(var chunk of level.chunks){
                    if(chunk == drag.chunk) continue;
                    var l = chunk.position[0] - chunk.width/2 - drag.chunk.width/2;
                    var r = chunk.position[0] + chunk.width/2 + drag.chunk.width/2;
                    var d = chunk.position[1] - chunk.height/2 - drag.chunk.height/2;
                    var u = chunk.position[1] + chunk.height/2 + drag.chunk.height/2;
                    if(l < drag.chunk.position[0] && drag.chunk.position[0] < r
                       && d < drag.chunk.position[1] && drag.chunk.position[1] < u){
                        if(Math.abs(drag.chunk.position[0] - chunk.position[0])
                           < Math.abs(drag.chunk.position[1] - chunk.position[1])){
                            if(drag.chunk.position[1] < chunk.position[1])
                                drag.chunk.position[1] = d;
                            else
                                drag.chunk.position[1] = u;
                        }else{
                            if(drag.chunk.position[0] < chunk.position[0])
                                drag.chunk.position[0] = l;
                            else
                                drag.chunk.position[0] = r;
                        }
                        found = true;
                        break;
                    }
                }
            }
            updatePosition(drag.entry, drag.chunk);
            console.log("Moved",drag.chunk,"to",drag.chunk.position);
            drag = null;
        }
    });
};

var toggleLevel = function(level){
    var tilemap = document.querySelector(".main>section.tilemap");
    var levelmap = document.querySelector(".main>section.levelmap");
    if(levelmap.style.display == "none"){
        level.uiElement.querySelector("header").classList.add("selected");
        levelmap.style.display = "block";
        tilemap.style.display = "none";
        generateLevelmap(level);
    }else{
        level.uiElement.querySelector("header").classList.remove("selected");
        levelmap.style.display = "none";
        tilemap.style.display = "flex";
    }
};

var createSolidTileset = function(){
    var tileToRG = [[]];
    var RGToTile = {};
    var pixels = new ImageData(tileSize*32, tileSize);
    var x=0, y=0;
    for(var key in solids){
        var color = solids[key];
        var r = (color & 0x000000FF) >>>  0;
        var g = (color & 0x0000FF00) >>>  8;
        var b = (color & 0x00FF0000) >>> 16;
        var a = (color & 0xFF000000) >>> 24;
        tileToRG[0][x] = [r, g, b];
        RGToTile[(r<<8)+g] = [x, 0];
        for(var i=y*tileSize; i<(y+1)*tileSize; i++){
            for(var j=x*tileSize; j<(x+1)*tileSize; j++){
                var index = (j+i*pixels.width)*4;
                pixels.data[index+0] = r;
                pixels.data[index+1] = g;
                pixels.data[index+2] = b;
                pixels.data[index+3] = a;
            }
        }
        x++;
    }
    return getImage(pixels)
        .then(function(image){
            return new Tileset({
                name: "solids",
                pixels: pixels,
                image: image,
                tileMap: tileToRG,
                rgMap: RGToTile
            });
        });
};

var selectTileEvent = function(ev){
    var tileset = level.chunk.getTileset();
    if(ev instanceof WheelEvent){
        var delta = -Math.sign(ev.deltaY);
        var i = tileset.selected[0]+tileset.selected[1]*tileset.tilesPerRow;
        i += delta;
        tileset.selected = [i % tileset.tilesPerRow,
                            Math.floor(i / tileset.tilesPerRow)];
    }else if(ev instanceof MouseEvent){
        var x = Math.floor(ev.offsetX/tilelist.clientWidth*tilelist.width/tileSize);
        var y = Math.floor(ev.offsetY/tilelist.clientHeight*tilelist.height/tileSize);
        tileset.selected = [x, y];
    }
};

var button = 0;
var editMapEvent = function(ev){
    ev.preventDefault();
    var x = Math.floor(ev.offsetX/tilemap.clientWidth*tilemap.width/tileSize);
    var y = Math.floor(ev.offsetY/tilemap.clientHeight*tilemap.height/tileSize);
    if(ev.buttons){
        if(ev.altKey || ev.metaKey){
            var pixelIndex = ((level.chunk.layer.width*y)+x)*4;
            var r = level.chunk.layer.data[pixelIndex+0];
            var g = level.chunk.layer.data[pixelIndex+1];
            console.log(r, g);
            level.chunk.tileset.selected = level.chunk.tileset.rgMap[(r<<8) + g];
        }else if(ev.ctrlKey){
            level.chunk.edit(x, y, "fill");
        }else{
            var action = (button == 2)? "erase" : "place";
            level.chunk.edit(x, y, action);
        }
    }
    if(ev.altKey || ev.metaKey){
        tilemap.style.cursor = "crosshair";
    }else{
        tilemap.style.cursor = "auto";
    }
    document.querySelector("#posXY").innerText = pad(x, 3)+"x"+pad(y, 3);
    return false;
};

var zoomEvent = function(){
    level.zoom(parseFloat(document.querySelector("#zoom").value));
};

var openTileset = function(){
    return openFile(".png,image/png")
        .then(async input => {
            var tileset = await loadTileset(input);
            for(var chunk of level.chunks){
                if(chunk.tileset.name == tileset.name){
                    chunk.tileset = tileset;
                    if(level.chunk == chunk)
                        chunk.show();
                }
            }
            return tileset.use();
        });
};

var newChunk = function(level){
    return showPrompt(".prompt.chunk", {
        "#chunk-height": 26,
        "#chunk-width": 40})
        .then(async (prompt)=>{
            var name = prompt.querySelector("#chunk-name").value;
            var w = parseInt(prompt.querySelector("#chunk-width").value);
            var h = parseInt(prompt.querySelector("#chunk-height").value);
            var outside = prompt.querySelector("#chunk-outside").checked;
            var tileset = level.defaultTileset;
            var background = null;
            if(prompt.querySelector("#chunk-tileset").value)
                tileset = await loadTileset(prompt.querySelector("#chunk-tileset"));
            if(prompt.querySelector("#chunk-background").value)
                await loadFile(prompt.querySelector("#chunk-background"), "data")
                .then(loadImage, (e)=>showPrompt(".prompt.error", e))
                .then(image => {
                    image.name = image.name.substring(0, image.name.lastIndexOf("."));
                    background = image;
                });
            level.chunks.push(
                    new Chunk({
                        name: name,
                        width: w,
                        height: h,
                        tileset: tileset,
                        background: background
                    }));
            generateSidebar(level);
            generateLevelmap(level);
            return level.chunks[level.chunks.length-1];
        });
};

var editChunk = function(chunk){
    return showPrompt(".prompt.chunk", {
        "#chunk-name": chunk.name,
        "#chunk-width": chunk.width,
        "#chunk-height": chunk.height,
        "#chunk-outside": {checked: chunk.outside},
        "#chunk-storyitems": printStory(chunk.storyItems),
        "#chunk-tileset+img": {src: chunk.tileset.image.src},
        "#chunk-background+img": {src: (chunk.background)?chunk.background.src:""}})
        .then(async (prompt)=>{
            chunk.name = prompt.querySelector("#chunk-name").value;
            chunk.storyItems = parseStory(prompt.querySelector("#chunk-storyitems").value);
            chunk.outside = prompt.querySelector("#chunk-outside").checked;
            chunk.resize(parseInt(prompt.querySelector("#chunk-width").value),
                         parseInt(prompt.querySelector("#chunk-height").value));
            chunk.uiElement.querySelector("header label").innerText = chunk.name;
            if(prompt.querySelector("#chunk-tileset").value)
                chunk.tileset = await loadTileset(prompt.querySelector("#chunk-tileset"));
            if(prompt.querySelector("#chunk-background").value)
                await loadFile(prompt.querySelector("#chunk-background"), "data")
                .then(loadImage, (e)=>showPrompt(".prompt.error", e))
                .then(image => {
                    image.name = image.name.substring(0, image.name.lastIndexOf("."));
                    chunk.background = image;
                });
            return chunk;
        });
};

var newLevel = function(){
    return showPrompt(".prompt.level")
        .then(async (prompt)=>{
            var preview = null;
            var tileset = null;
            if(prompt.querySelector("#level-preview").value)
                await loadFile(prompt.querySelector("#level-preview"), "data")
                .then(loadImage, (e)=>showPrompt(".prompt.error", e))
                .then(image => preview = image);
            if(prompt.querySelector("#level-tileset").value)
                tileset = await loadTileset(prompt.querySelector("#level-tileset"));
            return new Level({
                name: prompt.querySelector("#level-name").value,
                description: prompt.querySelector("#level-description").value,
                next: prompt.querySelector("#level-next").value,
                preview: preview,
                startChase: prompt.querySelector("#level-startchase").checked,
                defaultTileset: tileset,
            });});
};

var editLevel = function(){
    return showPrompt(".prompt.level", {
        "#level-name": level.name,
        "#level-description": level.description,
        "#level-next": level.next || "",
        "#level-startchase": {checked: level.startChase},
        "#level-tileset+img": {src: (level.defaultTileset)?level.defaultTileset.image.src:""},
        "#level-preview+img": {src: (level.preview)?level.preview.src:""}})
        .then(async (prompt)=>{
            level.name = prompt.querySelector("#level-name").value;
            level.description = prompt.querySelector("#level-description").value;
            level.next = prompt.querySelector("#level-next").value || null;
            level.startChase = prompt.querySelector("#level-startchase").checked;
            level.uiElement.querySelector("header label").innerText = level.name;
            if(prompt.querySelector("#level-preview").value)
                await loadFile(prompt.querySelector("#level-preview"), "data")
                .then(loadImage, (e)=>showPrompt(".prompt.error", e))
                .then(image => level.preview = image);
            if(prompt.querySelector("#level-tileset").value)
                level.defaultTileset = await loadTileset(prompt.querySelector("#level-tileset"));
            return level;
        });
};

var openLevel = function(){
    return openFile(".zip,application/zip")
        .then(input => loadFile(input, "array"))
        .then(([data]) => {
            var zip = new JSZip();
            var tilesets = [];
            return zip.loadAsync(data)
                .then(zip => zip.file("level.json").async("string"))
                .catch((e)=>showPrompt(".prompt.error", e))
                .then(async data => {
                    var json = JSON.parse(data);
                    for(var i=0; i<json.chunks.length; i++){
                        var chunk = json.chunks[i];
                        if(tilesets.indexOf(chunk.tileset) < 0) tilesets.push(chunk.tileset);
                        chunk.position = [ chunk.position[0]/tileSize, chunk.position[1]/tileSize ];
                        chunk.pixels = [];
                        chunk.tileset = new Tileset({name: chunk.tileset});
                        if(chunk.background){
                            var name = chunk.background;
                            chunk.background = new Image();
                            chunk.background.name = name;
                        }
                        for(var l=0; l<chunk.layers.length; l++){
                            var base64 = await zip.file(chunk.layers[l]).async("base64");
                            var image = await loadImage("data:image/png;base64,"+base64);
                            chunk.pixels[l] = getImagePixels(image);
                        }
                        json.chunks[i] = new Chunk(chunk);
                    }
                    if(json.preview){
                        var preview = await zip.file(json.preview).async("base64");
                        json.preview = await loadImage("data:image/png;base64,"+preview);
                    }
                    alert("Please load the tilesets \""+tilesets.join("\", \"")+"\"");
                    return new Level(json).use();
                });});
};

var saveLevel = function(){
    var zip = new JSZip();
    var data = level.serialize();
    for(var c=0; c<level.chunks.length; ++c){
        let chunk = level.chunks[c];
        for(var l=0; l<chunk.layers; ++l){
            var image = getImageBase64(chunk.pixels[l]);
            zip.file(data.chunks[c].layers[l], image, {"base64":true});
        }
    }
    if(level.preview)
        zip.file(data.preview, getImageBase64(level.preview), {"base64":true});
    zip.file("level.json", JSON.stringify(data));
    zip.generateAsync({type:"base64"}).then(function(data){
        data = "data:application/zip;base64,"+data;
        return saveFile(data, level.name+".zip");
    });
};

var showPrompt = function(id, defaults){
    defaults = defaults || {};
    var prompts = document.querySelector("#prompts");
    var prompt = prompts.querySelector(id);
    var ok = prompt.querySelector("input[type=submit]");
    if(typeof defaults === 'string' || defaults instanceof String){
        defaults = {".message": {innerText: defaults}};
    }
    return new Promise((accept, reject) =>{
        var fail, succeed;
        var cleanup = ()=>{
            prompts.style.display = "none";
            prompt.style.display = "none";
            prompts.removeEventListener("click", fail);
            window.removeEventListener("keyup", fail);
            ok.removeEventListener("click", succeed);
        };
        fail = (ev)=>{
            if((ev instanceof KeyboardEvent && ev.key == "Escape")
               || (ev instanceof MouseEvent && ev.target == prompts)){
                cleanup();
                if(reject)reject(prompt, ev);
            }
        };
        succeed = (ev)=>{
            if(prompt.checkValidity()){
                cleanup();
                accept(prompt, ev);
            }
            ev.preventDefault();
            return false;
        };
        prompts.addEventListener("click", fail);
        window.addEventListener("keyup", fail);
        ok.addEventListener("click", succeed);

        [].forEach.call(prompt.querySelectorAll("input"), (el)=>{
            switch(el.getAttribute("type")){
            case "checkbox": el.checked = false; break;
            case "submit": break;
            default: el.value = ""; break;
            }
        });
        for(var selector in defaults){
            var element = prompt.querySelector(selector);
            var value = defaults[selector];
            if(value.constructor !== Object)
                value = {value: value};
            for(var key in value)
                element[key] = value[key];
        }
        
        prompt.style.display = "block";
        prompts.style.display = "flex";
        prompt.querySelector("input").focus();
    });
};

var stopUnload = function(e){
    e.preventDefault();
    var confirmationMessage = "Are you sure you want to leave? Unsaved progress will be lost.";
    e.returnValue = confirmationMessage;
    return confirmationMessage;
};

var initEvents = function(){
    document.querySelector("#new-level").addEventListener("click", newLevel);
    document.querySelector("#open-level").addEventListener("click", openLevel);
    document.querySelector("#save-level").addEventListener("click", saveLevel);
    document.querySelector("#open-tileset").addEventListener("click", openTileset);
    document.querySelector("#zoom").addEventListener("change", zoomEvent);
    document.querySelector("label[for=zoom]").addEventListener("dblclick", ()=> level.zoom(1.0));
    window.addEventListener("wheel", selectTileEvent);
    window.onbeforeunload = stopUnload;
    window.addEventListener("beforeunload", stopUnload);
    tilelist.addEventListener("click", selectTileEvent);
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
    
    listctx = tilelist.getContext("2d");
    listctx.mozImageSmoothingEnabled = false;
    listctx.webkitImageSmoothingEnabled = false;
    listctx.msImageSmoothingEnabled = false;
    listctx.imageSmoothingEnabled = false;
};

var init = function(){
    console.log("Init.");
    
    initEvents();
    initCanvas();
    
    createSolidTileset()
        .then((tileset)=>{
            solidset = tileset;
            new Level().use();
        });
};

init();
