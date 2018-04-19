# How the hell do I deploy?

## TL;DR
You need to globally install "copyfiles" and "ts-node": `npm i -g copyfiles ts-node`
Execute this from the `server` folder: `npm run deploy`.

## Long story
The functions are coded in typescript and the rules in bolt. In order to deploy to firebase, you need to transpile the functions code to javascript and the rules code to a json file. 

The functions transpilation is done using `tsc`. The resulting code is put under the dist folder.  The rules transpilation using google's bolt compiler. The resulting code is put at the server's root. 

We specify in the `firebase.json` configuration folder what files to use.