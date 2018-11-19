const { app, BrowserWindow } = require("electron")
const express = require("express")
const bp = require("body-parser")
const os = require("os")
const server = express()
const path = require("path")
const proc = require("child_process").spawn
const axios = require("axios")

let apipath = path.join(
  __dirname,
  process.env.NODE_ENV !== "development"
    ? "..\\..\\..\\api\\bin\\dist\\win\\api.exe"
    : "..\\..\\api\\bin\\dist\\win\\api.exe",
)
console.log(apipath)
if (os.platform() === "darwin")
  apipath = path.join(
    __dirname,
    process.env.NODE_ENV !== "development"
      ? "..//..//..//api//bin//dist//osx//api"
      : "..//..//api//bin//dist//osx//api",
  )

const Sentry = require("@sentry/node")
Sentry.init({ dsn: "https://dc7c9a8085d64d6d9181158bc19e3236@sentry.io/1323916" })

server.use(bp.json())

const missedRequests = []
let finishload = false
server.use((req, res) => {
  if (finishload) mainWindow.webContents.send("request", req)
  else missedRequests.push(req)
  if (req.url === "/init")
    axios
      .post(`http://localhost:1548/api`)
      .then(e => {
        mainWindow.show()
      })
      .catch(e => console.error(e))
  res.send(200)
})
server.listen(1549)

/**
 * Set `__static` path to static files in production
 * https://simulatedgreg.gitbooks.io/electron-vue/content/en/using-static-assets.html
 */
if (process.env.NODE_ENV !== "development")
  global.__static = require("path")
    .join(__dirname, "/static")
    .replace(/\\/g, "\\\\")

let mainWindow
const winURL =
  process.env.NODE_ENV === "development"
    ? `http://localhost:9080`
    : `file://${__dirname}/index.html`

function createWindow() {
  /**
   * Initial window options
   */
  mainWindow = new BrowserWindow({
    width: 750,
    // x: 0,
    // y: 0,
    minWidth: 500,
    height: 292,
    width: 685,
    backgroundColor: "#212121",
    frame: false,
    show: false,
    center: true,
    minHeight: 292,
    maxHeight: 292,
    alwaysOnTop: true,
    webPreferences: { webSecurity: false },
  })
  // nodeIntegration: false, // TODO: fix this security thing

  mainWindow.loadURL(winURL)

  mainWindow.webContents.on("did-finish-load", () => {
    finishload = true
    missedRequests.forEach(req => mainWindow.webContents.send("request", req))
  })
  // mainWindow.on("closed", () => {
  //   mainWindow = null
  // })

  // mainWindow.once("ready-to-show", () => {
  //   mainWindow.show()
  // })
}

// app.on("ready", createWindow)
app.on("ready", startApi)

app.on("window-all-closed", () => {
  if (process.platform !== "darwin") app.quit()
})

app.on("activate", () => {
  if (mainWindow === null) createWindow()
})

function startApi() {
  createWindow()
  const apiProcess = proc(apipath)
  apiProcess.stdout.on("data", data => {
    writeLog(`api: ${data}`)
  })
}
process.on("exit", () => {
  writeLog("exit")
  apiProcess.kill()
})

function writeLog(msg) {
  console.log(msg)
}
/**
 * Auto Updater
 *
 * Uncomment the following code below and install `electron-updater` to
 * support auto updating. Code Signing with a valid certificate is required.
 * https://simulatedgreg.gitbooks.io/electron-vue/content/en/using-electron-builder.html#auto-updating
 */

/*
import { autoUpdater } from 'electron-updater'

autoUpdater.on('update-downloaded', () => {
autoUpdater.quitAndInstall()
})

app.on('ready', () => {
if (process.env.NODE_ENV === 'production') autoUpdater.checkForUpdates()
})
*/
