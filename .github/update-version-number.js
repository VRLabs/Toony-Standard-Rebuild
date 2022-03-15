const fs = require('fs');
const path = require('path');

let folder = "Editor"
if (!process.argv[2]) {
    console.log("a version number is needed");
    return;
}

if (process.argv[3]) {
    folder = process.argv[3];
}

function filewalker(dir, done) {
    let results = [];

    fs.readdir(dir, function(err, list) {
        if (err) return done(err);

        var pending = list.length;

        if (!pending) return done(null, results);

        list.forEach(function(file){
            file = path.resolve(dir, file);

            fs.stat(file, function(err, stat){
                // If directory, execute a recursive call
                if (stat && stat.isDirectory()) {
                    // Add directory to array [comment if you need to remove the directories from the array]
                    results.push(file);

                    filewalker(file, function(err, res){
                        results = results.concat(res);
                        if (!--pending) done(null, results);
                    });
                } else {
                    results.push(file);

                    if (!--pending) done(null, results);
                }
            });
        });
    });
}

try {

    filewalker(folder, function(err, data){
        if(err){
            throw err;
        }

        data.forEach(element => {
            if (element.substring(element.lastIndexOf('.') + 1) == "asset") {
                let fileContents = fs.readFileSync(element, 'utf8');
                fs.writeFileSync(element, fileContents.replace(/(?<=Version: ).*(?=(\n|\v|\r)\s*Author: )/gm, process.argv[2]));
            }
        });
    });
    
} catch (e) {
    console.log(e);
}