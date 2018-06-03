const fs = require("fs");
function parseLabels(file, collector, done) {
    const strict = true, options = false;
    let saxStream = require("sax").createStream(strict, options);
    let make = () => new Label();
    // let textNodes = ["id", "name", "contactInfo", "profile", "data_quality", "url"];
    let ignoreNodes=["images", "sublabels"];
    let textBuffer = "";
    let currentNode = "";
    let currentModel = make();
    let mainNode = "label";
    const nodeMap = {
        "id": (l, v) => { l.id = parseInt(v); },
        "name": (l, v) => { l.name = v; },
        "contactInfo": (l, v) => { l.contactInfo = v; },
        "profile": (l, v) => { l.profile = v; },
        "data_quality": (l, v) => { l.dataQuality = v; },
        "url": (l, v) => { l.urls.push(v); },
    };
    let setText = (model, name, text) => {
        if (name in nodeMap) {
            nodeMap[name](model, text);
        } else {
            // console.error(`Could not find map for: ${name}.`);
        }
    };

    saxStream.on("error", e => {
        // unhandled errors will throw, since this is a proper node
        // event emitter.
        console.error("parser error!", e);
        // clear the error
        this._parser.error = null;
        this._parser.resume();
    });
    saxStream.on("opentag", node => {
        // console.debug(`> ${node.name}`);
        // if we're currently inside an ignore node, we'll skip this start node
        if (ignoreNodes.includes(currentNode)) {
            textBuffer = "";
            return;
        }
        currentNode = node.name;
        // we've encountered an ignorable node
        if (ignoreNodes.includes(currentNode)) {
            textBuffer = "";
            return;
        }
        if (currentNode == mainNode) {
            // starting new node
            currentModel = make();
            textBuffer = "";
        }
    });
    saxStream.on("closetag", nodeName => {
        // if name is in _ignore_nodes, it means we're done ignoring
        if (ignoreNodes.includes(nodeName)) {
            currentNode = null;
            textBuffer = "";
            return;
        }
        // we're inside an ignore node, we don't care about processing
        if (ignoreNodes.includes(currentNode)) {
            textBuffer = "";
            return;
        }
        // if it's the end of the main node, let's collect it
        if (nodeName == mainNode) {
            if (currentModel.id == 0) console.debug(`id=0 on node ${nodeName}.`);
            collector.collect(currentModel);
            textBuffer = "";
            return;
        }
        // it means we're in a regular node
        setText(currentModel, nodeName, textBuffer);
    });
    saxStream.on("text", text => {
        textBuffer = text;
    });

    saxStream.on("end", () => { 
        console.log("Stream done");
        done();
    });
    // pipe is supported, and it's readable/writable
    // same chunks coming in also go out.
    fs.createReadStream(file)
        .pipe(saxStream);
    // .pipe(fs.createWriteStream("file-copy.xml"))
}

class Label {
    constructor() {
        this.id = 0;
        this.dataQuality = null;
        this.name = null;
        this.contactInfo = null;
        this.profile = null;
        this.urls = [];
        this.sublabels = [];
        this.parentLabel = null;
    }
}

class Artist { // eslint-disable-line no-unused-vars
    constructor() {
        this.id = 0;
        this.dataQuality = null;
        this.name = null;
        this.realName = null;
        this.profile = null;
        this.urls = [];
        this.aliases = [];
        this.nameVariations = [];
        this.members = [];
        this.groups = [];
    }
}

class StatsCollector {
    constructor() {
        this.dataQualityLedger = {
            "Needs Vote": 0,
            "Complete and Correct": 0,
            "Correct": 0,
            "Needs Minor Changes": 0,
            "Needs Major Changes": 0,
            "Entirely Incorrect": 0,
            "Entirely Incorrect Edit": 0
        };
        this.numberFormatter = new Intl.NumberFormat(
            "en-US",
            { useGrouping: true }
        );
    }

    collect(subject) {
        if (subject.id == 0) return;
        // console.debug(`Collect ${subject.id}: ${subject.dataQuality}`);
        let quality = subject.dataQuality;
        if (quality in this.dataQualityLedger) {
            this.dataQualityLedger[quality] += 1;
        }
    }

    printStats(logger) {
        let total = 0;
        for(let key in this.dataQualityLedger) {
            total += this.dataQualityLedger[key];
        }
        logger.log(`Total: ${total} entries.`);
        let orderedKeys = Object.keys(this.dataQualityLedger).sort();

        orderedKeys.forEach(key => {
            let val = this.dataQualityLedger[key];
            logger.log(`  ${key.padEnd(25)} = ${this.numberFormatter.format(val).padStart(10)}`);
        });
    }
}


function main(file) {
    console.debug(`Parsing: ${file}.`);
    let stats = new StatsCollector();
    if (file.indexOf("labels") > -1) {
        console.debug("Performing labels test");
        console.time("parseLabels");
        parseLabels(file, stats, () => {
            console.timeEnd("parseLabels");
            stats.printStats(console);
        });
    }
    // stats.printStats(console);
}

if (!module.parent) {
    main(process.argv[2]);
}