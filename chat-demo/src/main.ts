import {
  HubConnectionBuilder,
  HubConnection,
  LogLevel,
} from "@microsoft/signalr";

type StoryPartDto = {
  id: number;
  text: string;
  createdDate: string;
  userName: string;
};

const els = {
  baseUrl: document.getElementById("baseUrl") as HTMLInputElement,
  jwtToken: document.getElementById("jwtToken") as HTMLInputElement,
  connectBtn: document.getElementById("connectBtn") as HTMLButtonElement,
  disconnectBtn: document.getElementById("disconnectBtn") as HTMLButtonElement,
  connectionState: document.getElementById("connectionState") as HTMLElement,
  reconnectInfo: document.getElementById("reconnectInfo") as HTMLElement,

  storyId: document.getElementById("storyId") as HTMLInputElement,
  joinBtn: document.getElementById("joinBtn") as HTMLButtonElement,
  leaveBtn: document.getElementById("leaveBtn") as HTMLButtonElement,

  currentAuthor: document.getElementById("currentAuthor") as HTMLElement,
  turnCountdown: document.getElementById("turnCountdown") as HTMLElement,

  storyPartText: document.getElementById("storyPartText") as HTMLTextAreaElement,
  sendBtn: document.getElementById("sendBtn") as HTMLButtonElement,

  log: document.getElementById("log") as HTMLDivElement,
};

let connection: HubConnection | null = null;
let turnEndTimeUtc: Date | null = null;
let countdownTimer: number | null = null;

function logLine(message: string, cssClass?: string) {
  const p = document.createElement("div");
  if (cssClass) p.className = cssClass;
  const time = new Date().toLocaleTimeString();
  p.textContent = `[${time}] ${message}`;
  els.log.appendChild(p);
  els.log.scrollTop = els.log.scrollHeight;
}

function setConnectionState(
  state: "connected" | "connecting" | "disconnected" | "reconnecting"
) {
  els.connectionState.textContent = state;
  els.connectionState.className =
    "badge " + (state === "connected" ? "ok" : state === "reconnecting" ? "warn" : "");
}

function setUiConnected(connected: boolean) {
  els.connectBtn.disabled = connected;
  els.disconnectBtn.disabled = !connected;
  els.joinBtn.disabled = !connected;
  els.leaveBtn.disabled = !connected;
  els.sendBtn.disabled = !connected;
}

function getStoryId(): number | null {
  const id = Number(els.storyId.value);
  return Number.isFinite(id) && id > 0 ? id : null;
}

function startTurnCountdown() {
  if (!turnEndTimeUtc) {
    els.turnCountdown.textContent = "-";
    return;
  }
  stopTurnCountdown();
  const update = () => {
    if (!turnEndTimeUtc) return;
    const now = new Date();
    const ms = turnEndTimeUtc.getTime() - now.getTime();
    if (ms <= 0) {
      els.turnCountdown.textContent = "0s";
      stopTurnCountdown();
      return;
    }
    const sTotal = Math.floor(ms / 1000);
    const min = Math.floor(sTotal / 60);
    const sec = sTotal % 60;
    els.turnCountdown.textContent = `${min}m ${sec}s`;
  };
  update();
  countdownTimer = window.setInterval(update, 500);
}

function stopTurnCountdown() {
  if (countdownTimer != null) {
    window.clearInterval(countdownTimer);
    countdownTimer = null;
  }
}

async function connect() {
  if (connection) {
    logLine("Already connected or connecting.", "warn");
    return;
  }

  const baseUrl = els.baseUrl.value.trim();
  const token = els.jwtToken.value.trim();
  if (!baseUrl) {
    logLine("Base URL is required.", "err");
    return;
  }
  if (!token) {
    logLine("JWT token is required to connect (hub is authorized).", "err");
    return;
  }

  setConnectionState("connecting");
  setUiConnected(false);
  els.reconnectInfo.textContent = "";

  const hubUrl = `${baseUrl.replace(/\/+$/, "")}/story-hub`;

  connection = new HubConnectionBuilder()
    .withUrl(hubUrl, {
      accessTokenFactory: () => token,
    })
    .withAutomaticReconnect()
    .configureLogging(LogLevel.Information)
    .build();

  // Server-to-client handlers
  connection.on("ReceiveStoryPart", (storyId: number, created: StoryPartDto) => {
    console.log("ReceiveStoryPart payload:", created);
    logLine(
      `ReceiveStoryPart | storyId=${storyId}, by=${created.userName} | "${created.text}"`
    );
  });

  connection.on("MessageFailed", (message: string) => {
    logLine(`MessageFailed | ${message}`, "err");
  });

  connection.on("UserConnected", (userName: string) => {
    logLine(`UserConnected | ${userName}`, "ok");
  });

  connection.on("UserDisconnected", (userName: string) => {
    logLine(`UserDisconnected | ${userName}`, "warn");
  });

  connection.on(
    "SetInitialState",
    (currentAuthorUsername: string | null, turnEndTime: string) => {
      els.currentAuthor.textContent = currentAuthorUsername ?? "-";
      turnEndTimeUtc = turnEndTime ? new Date(turnEndTime) : null;
      startTurnCountdown();
      logLine(
        `SetInitialState | currentAuthor=${currentAuthorUsername ?? "-"}; turnEnd=${
          turnEndTimeUtc?.toISOString() ?? "-"
        }`
      );
    }
  );

  connection.on(
    "ReceiveTurnChange",
    (newAuthorUsername: string, turnEndTime: string) => {
      els.currentAuthor.textContent = newAuthorUsername ?? "-";
      turnEndTimeUtc = turnEndTime ? new Date(turnEndTime) : null;
      startTurnCountdown();
      logLine(
        `ReceiveTurnChange | newAuthor=${newAuthorUsername}; nextEnd=${
          turnEndTimeUtc?.toISOString() ?? "-"
        }`,
        "warn"
      );
    }
  );

  // Lifecycle hooks
  connection.onreconnecting((err) => {
    setConnectionState("reconnecting");
    els.reconnectInfo.textContent = "Reconnecting…";
    logLine(`Reconnecting... ${err?.message ?? ""}`, "warn");
  });

  connection.onreconnected((connectionId) => {
    setConnectionState("connected");
    els.reconnectInfo.textContent = `Reconnected (${connectionId ?? "no id"})`;
    logLine(`Reconnected. ConnectionId=${connectionId ?? "-"}`, "ok");
  });

  connection.onclose((err) => {
    setConnectionState("disconnected");
    setUiConnected(false);
    logLine(`Connection closed. ${err?.message ?? ""}`, "warn");
  });

  try {
    await connection.start();
    setConnectionState("connected");
    setUiConnected(true);
    logLine("Connected.", "ok");
  } catch (e: any) {
    setConnectionState("disconnected");
    setUiConnected(false);
    logLine(`Failed to connect: ${e?.message ?? e}`, "err");
    connection = null;
  }
}

async function disconnect() {
  if (!connection) return;
  try {
    await connection.stop();
  } catch (e: any) {
    logLine(`Error on disconnect: ${e?.message ?? e}`, "err");
  } finally {
    connection = null;
    setConnectionState("disconnected");
    setUiConnected(false);
    els.currentAuthor.textContent = "-";
    turnEndTimeUtc = null;
    stopTurnCountdown();
    els.turnCountdown.textContent = "-";
    logLine("Disconnected.", "warn");
  }
}

async function joinSession() {
  if (!connection) {
    logLine("Connect first.", "warn");
    return;
  }
  const storyId = getStoryId();
  if (!storyId) {
    logLine("Invalid Story ID.", "err");
    return;
  }
  try {
    await connection.invoke("JoinStorySession", storyId);
    logLine(`JoinStorySession invoked for storyId=${storyId}`, "ok");
  } catch (e: any) {
    logLine(`JoinStorySession failed: ${e?.message ?? e}`, "err");
  }
}

async function leaveSession() {
  if (!connection) {
    logLine("Connect first.", "warn");
    return;
  }
  const storyId = getStoryId();
  if (!storyId) {
    logLine("Invalid Story ID.", "err");
    return;
  }
  try {
    await connection.invoke("LeaveStorySession", storyId);
    logLine(`LeaveStorySession invoked for storyId=${storyId}`, "ok");
  } catch (e: any) {
    logLine(`LeaveStorySession failed: ${e?.message ?? e}`, "err");
  }
}

async function sendStoryPart() {
  if (!connection) {
    logLine("Connect first.", "warn");
    return;
  }
  const storyId = getStoryId();
  if (!storyId) {
    logLine("Invalid Story ID.", "err");
    return;
  }
  const text = els.storyPartText.value.trim();
  if (!text) {
    logLine("Cannot send empty story part.", "err");
    return;
  }
  try {
    await connection.invoke("SendStoryPart", storyId, text);
    logLine(`SendStoryPart invoked.`, "ok");
    els.storyPartText.value = "";
  } catch (e: any) {
    logLine(`SendStoryPart failed: ${e?.message ?? e}`, "err");
  }
}

// Wire UI
els.connectBtn.addEventListener("click", connect);
els.disconnectBtn.addEventListener("click", disconnect);
els.joinBtn.addEventListener("click", joinSession);
els.leaveBtn.addEventListener("click", leaveSession);
els.sendBtn.addEventListener("click", sendStoryPart);

// Defaults
els.baseUrl.value = "http://localhost:5014";
setConnectionState("disconnected");
setUiConnected(false);