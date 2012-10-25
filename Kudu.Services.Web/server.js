var path = require("path");

var port = process.env.PORT || 8080;
var vfsPath = "/vfs/";

var tmp = process.env.TMP;

// Normally, finding the app is done by calling MapPath("_app"), but we can't do this from Node.
// So for now, finding using hacks that work on Azure and on local Kudu

var root = process.cwd();
if (tmp.substring(0, 18).toLowerCase() == "c:\\dwasfiles\\sites") {
    // This works in Azure because TMP is part of the app's tree
    root = path.join(tmp, "..", "VirtualDirectory0");
}
else if (process.env.APP_POOL_ID) {
    // This works in local Kudu in the default case
    root = path.join(root, "..", "apps", process.env.APP_POOL_ID)
}
else {
    // This happens when running directly from cmd line
    console.log("Couldn't identify app path. Defaulting to the process cwd!");
}

root = path.normalize(root);
console.log("root=" + root);

var vfs = require('vfs-local')({
    root: root
});

require('http').createServer(require('stack')(
  require('vfs-http-adapter')(vfsPath, vfs)
)).listen(port);

console.log("vfs at " + "http://localhost:" + port + vfsPath);
