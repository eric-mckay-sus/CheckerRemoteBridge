# Plan

## General design, adapt to light mode & styles in Blazor app

```html

<style>
  @import url('https://fonts.googleapis.com/css2?family=JetBrains+Mono:wght@400;500&family=Syne:wght@400;500;600&display=swap');
  *{box-sizing:border-box;margin:0;padding:0}
  :root{
    --c-bg:#0f1117;--c-panel:#161b27;--c-card:#1c2232;--c-border:#2a3148;
    --c-text:#e2e8f0;--c-muted:#64748b;--c-mono:'JetBrains Mono',monospace;--c-sans:'Syne',sans-serif;
  }
  body{background:var(--c-bg);color:var(--c-text);font-family:var(--c-sans);padding:0 0 24px 0;min-height:100vh}
  h2.sr-only{position:absolute;width:1px;height:1px;overflow:hidden;clip:rect(0,0,0,0)}
  .top-bar{display:flex;align-items:center;justify-content:space-between;padding:14px 20px;border-bottom:1px solid var(--c-border);background:var(--c-panel)}
  .top-bar-title{font-size:13px;font-weight:600;letter-spacing:.06em;color:var(--c-muted);text-transform:uppercase}
  .server-badge{font-family:var(--c-mono);font-size:10px;color:var(--c-muted);background:#0c1020;padding:4px 10px;border-radius:4px;border:1px solid var(--c-border)}
  .live-dot{width:6px;height:6px;border-radius:50%;background:#10b981;display:inline-block;margin-right:6px;animation:pulse 2s infinite}
  @keyframes pulse{0%,100%{opacity:1}50%{opacity:.4}}
  .grid{display:grid;grid-template-columns:repeat(5,1fr);gap:10px;padding:16px}
  .pi-card{background:var(--c-card);border:1px solid var(--c-border);border-radius:10px;overflow:hidden;cursor:pointer;transition:border-color .2s,transform .15s;position:relative}
  .pi-card:hover{border-color:#4a5568;transform:translateY(-1px)}
  .pi-card.selected{border-color:#3b82f6;box-shadow:0 0 0 1px #3b82f664}
  .health-strip{height:3px;width:100%}
  .card-header{padding:9px 11px 7px;display:flex;justify-content:space-between;align-items:center}
  .card-title{font-size:11px;font-weight:600;letter-spacing:.05em;color:var(--c-muted);text-transform:uppercase}
  .health-pill{font-family:var(--c-mono);font-size:8.5px;font-weight:500;padding:2px 6px;border-radius:3px;letter-spacing:.03em;display:flex;align-items:center;gap:4px}
  .health-dot{width:5px;height:5px;border-radius:50%}
  .card-body{padding:0 11px 11px}
  .state-label{font-size:11.5px;font-weight:500;margin-bottom:4px;line-height:1.3}
  .msg-line{font-family:var(--c-mono);font-size:8.5px;color:var(--c-muted);white-space:nowrap;overflow:hidden;text-overflow:ellipsis;line-height:1.6}
  .alarm-line{color:#ef4444}
  .checksum-row{display:flex;align-items:center;gap:5px;margin-top:7px;font-family:var(--c-mono);font-size:8px}
  .checksum-match{color:#10b981}.checksum-mismatch{color:#ef4444}.checksum-pending{color:#64748b}
  .wd-age{font-family:var(--c-mono);font-size:8px;color:var(--c-muted);margin-top:5px}
  .quick-actions{display:flex;gap:4px;margin-top:8px}
  .qbtn{flex:1;font-size:8px;font-family:var(--c-mono);padding:4px 0;text-align:center;border-radius:4px;border:1px solid var(--c-border);background:#0c1020;color:var(--c-muted);cursor:pointer;transition:.15s}
  .qbtn:hover{border-color:#4a5568;color:var(--c-text)}
  .qbtn.danger:hover{border-color:#ef4444;color:#ef4444}
  .qbtn.warn{border-color:#7c2d12;color:#f97316;animation:urgentpulse 1.5s infinite}
  @keyframes urgentpulse{0%,100%{background:#0c1020}50%{background:#1f1008}}
  .section{padding:0 16px;margin-top:14px}
  .section-head{display:flex;justify-content:space-between;align-items:center;font-size:10px;font-weight:600;letter-spacing:.08em;color:var(--c-muted);text-transform:uppercase;margin-bottom:8px;padding-bottom:6px;border-bottom:1px solid var(--c-border)}
  .detail-panel{background:var(--c-card);border:1px solid var(--c-border);border-radius:10px;overflow:hidden;padding:14px}
  .detail-top{display:flex;justify-content:space-between;align-items:flex-start;margin-bottom:14px}
  .detail-name{font-size:15px;font-weight:600}
  .detail-sub{font-family:var(--c-mono);font-size:9.5px;color:var(--c-muted);margin-top:2px}
  .action-bar{display:flex;gap:8px;flex-wrap:wrap}
  .action-btn{font-size:10.5px;font-family:var(--c-sans);font-weight:500;padding:7px 13px;border-radius:6px;border:1px solid var(--c-border);background:#0c1020;color:var(--c-text);cursor:pointer;display:flex;align-items:center;gap:6px;transition:.15s}
  .action-btn:hover{border-color:#64748b}
  .action-btn.primary{border-color:#1e3a5f;background:#0c1d33;color:#60a5fa}
  .action-btn.danger{border-color:#7f1d1d;background:#1f0c0c;color:#ef4444}
  .action-btn.warn{border-color:#7c2d12;background:#1f1008;color:#f97316}
  .action-btn:disabled{opacity:.35;cursor:not-allowed}
  .dot{width:7px;height:7px;border-radius:50%;display:inline-block}
  .info-grid{display:grid;grid-template-columns:1fr 1fr;gap:1px;background:var(--c-border);margin-top:14px;border-radius:6px;overflow:hidden}
  .info-cell{background:var(--c-card);padding:11px 13px}
  .info-title{font-size:9px;font-weight:600;letter-spacing:.08em;color:var(--c-muted);text-transform:uppercase;margin-bottom:8px}
  .kv-row{display:flex;justify-content:space-between;padding:3px 0;font-family:var(--c-mono);font-size:9.5px}
  .kv-key{color:var(--c-muted)}
  .checksum-box{display:flex;justify-content:space-between;align-items:center;padding:6px 8px;border-radius:4px;margin-top:4px;font-family:var(--c-mono);font-size:9px}
  .log-list{font-family:var(--c-mono);font-size:9px;color:var(--c-muted);max-height:120px;overflow-y:auto}
  .log-entry{padding:3px 0;border-bottom:1px solid #1a2035;display:flex;justify-content:space-between}
  .badge{font-size:8px;font-family:var(--c-mono);padding:1px 5px;border-radius:3px}
  .status-strip{display:flex;gap:14px;padding:10px 16px;background:var(--c-panel);border-top:1px solid var(--c-border);margin-top:14px;flex-wrap:wrap}
  .legend-item{display:flex;align-items:center;gap:5px;font-size:9.5px;color:var(--c-muted)}
  .modal-overlay{min-height:0;background:rgba(0,0,0,0);display:flex;align-items:center;justify-content:center;transition:.2s}
  .modal-overlay.open{min-height:340px;background:rgba(0,0,0,.6)}
  .modal{background:var(--c-card);border:1px solid var(--c-border);border-radius:10px;padding:20px;width:320px;display:none}
  .modal-overlay.open .modal{display:block}
  .modal-title{font-size:13px;font-weight:600;margin-bottom:6px;display:flex;align-items:center;gap:8px}
  .modal-body{font-size:11px;color:var(--c-muted);line-height:1.6;margin-bottom:16px}
  .modal-actions{display:flex;gap:8px;justify-content:flex-end}
  .modal-btn{font-size:11px;font-weight:500;padding:7px 14px;border-radius:6px;border:1px solid var(--c-border);background:#0c1020;color:var(--c-text);cursor:pointer}
  .modal-btn.confirm-danger{border-color:#7f1d1d;background:#2a0d0d;color:#fca5a5}
  .modal-btn.confirm-warn{border-color:#7c2d12;background:#2a160d;color:#fdba74}
</style>

<h2 class="sr-only">Checker fleet operations console for five Raspberry Pi checker stations — health, watchdog age, checksum verification, and remote power actions</h2>

<div class="top-bar">
  <div>
    <span class="top-bar-title">Checker fleet — ops console</span>
    <span style="font-size:9px;color:var(--c-muted);margin-left:10px">5x Pi5 · Raspbian OS Full · OPC UA via KepServerEX</span>
  </div>
  <div style="display:flex;align-items:center;gap:12px">
    <span class="server-badge"><span class="live-dot"></span>SUS-KEPWARE-02:49320</span>
    <span id="clock" style="font-family:var(--c-mono);font-size:10px;color:var(--c-muted)"></span>
  </div>
</div>

<div class="grid" id="card-grid"></div>

<div class="section">
  <div class="section-head"><span id="detail-title">Final 1 — control panel</span></div>
  <div class="detail-panel">
    <div class="detail-top">
      <div>
        <div class="detail-name" id="detail-name">Final 1</div>
        <div class="detail-sub" id="detail-sub">ns=2;s=ANT1.Final1.Status</div>
      </div>
      <div id="detail-health"></div>
    </div>
    <div class="action-bar" id="action-bar"></div>
    <div class="info-grid" id="info-grid"></div>
  </div>
</div>

<div class="status-strip">
  <span style="font-size:10px;font-weight:600;color:var(--c-muted);margin-right:2px">Health:</span>
  <span class="legend-item"><span class="dot" style="background:#10b981"></span>Green — watchdog &lt;10s, no alarm</span>
  <span class="legend-item"><span class="dot" style="background:#ef4444"></span>Red — watchdog stale or alarm active</span>
  <span class="legend-item"><span class="dot" style="background:#475569"></span>Offline — CheckerState 0</span>
  <span class="legend-item"><span class="dot" style="background:#06b6d4"></span>Transitioning — 100/101/102</span>
</div>

<div class="modal-overlay" id="modal-overlay">
  <div class="modal" id="modal">
    <div class="modal-title" id="modal-title"></div>
    <div class="modal-body" id="modal-body"></div>
    <div class="modal-actions">
      <button class="modal-btn" onclick="closeModal()">Cancel</button>
      <button class="modal-btn" id="modal-confirm" onclick="confirmModal()">Confirm</button>
    </div>
  </div>
</div>

<script>
const STATE_INFO = {
  0:{label:'Offline / Not Ready',color:'#475569'},
  1:{label:'Alarm / Error',color:'#ef4444'},
  2:{label:'Idle — Manual Mode',color:'#f59e0b'},
  3:{label:'Idle — Auto Ready',color:'#3b82f6'},
  4:{label:'Test in Progress',color:'#8b5cf6'},
  5:{label:'Test — Ext. Step Wait',color:'#ec4899'},
  6:{label:'Test Complete',color:'#10b981'},
  7:{label:'Test Aborted',color:'#f97316'},
  100:{label:'Starting Up',color:'#06b6d4'},
  101:{label:'Shutting Down',color:'#06b6d4'},
  102:{label:'Rebooting',color:'#06b6d4'},
};

function ts(offsetSec=0){
  const d=new Date(Date.now()-offsetSec*1000);
  return d.getFullYear()+'-'+String(d.getMonth()+1).padStart(2,'0')+'-'+String(d.getDate()).padStart(2,'0')+' '+
    String(d.getHours()).padStart(2,'0')+':'+String(d.getMinutes()).padStart(2,'0')+':'+String(d.getSeconds()).padStart(2,'0');
}

const pis = [
  {id:1,state:3,statusMsg:'Idle — auto mode ready',alarmMsg:'',watchdogAge:3,expectedChecksum:'a8f3e91c',actualChecksum:'a8f3e91c',lastBoot:ts(5400),lastBackup:ts(1800),sshReachable:true},
  {id:2,state:4,statusMsg:'Testing part BEV-0042918 — Recipe 7',alarmMsg:'',watchdogAge:1,expectedChecksum:'b21c7740',actualChecksum:'b21c7740',lastBoot:ts(10200),lastBackup:ts(3600),sshReachable:true},
  {id:3,state:0,statusMsg:'No watchdog received',alarmMsg:'',watchdogAge:null,expectedChecksum:'c99a0123',actualChecksum:null,lastBoot:null,lastBackup:ts(7200),sshReachable:false},
  {id:4,state:1,statusMsg:'Tester in E-Stop',alarmMsg:'ESTOP input active on DI3',watchdogAge:34,expectedChecksum:'d77fbe45',actualChecksum:'9912ffaa',lastBoot:ts(86400),lastBackup:ts(900),sshReachable:true},
  {id:5,state:3,statusMsg:'Idle — auto mode ready',alarmMsg:'',watchdogAge:6,expectedChecksum:'e44021bb',actualChecksum:'e44021bb',lastBoot:ts(43200),lastBackup:ts(2400),sshReachable:true},
];

let selected = 0;

function health(p){
  if(p.state===0 || !p.sshReachable) return {code:'offline',label:'OFFLINE',color:'#475569'};
  if(p.state===100||p.state===101||p.state===102) return {code:'transition',label:'BUSY',color:'#06b6d4'};
  if(p.alarmMsg || p.watchdogAge===null || p.watchdogAge>10) return {code:'red',label:'RED',color:'#ef4444'};
  return {code:'green',label:'GREEN',color:'#10b981'};
}

function fmtAge(sec){
  if(sec===null) return 'no signal';
  if(sec<60) return sec+'s ago';
  if(sec<3600) return Math.floor(sec/60)+'m ago';
  return Math.floor(sec/3600)+'h ago';
}

function checksumStatus(p){
  if(p.actualChecksum===null) return {code:'pending',label:'awaiting boot report'};
  if(p.actualChecksum===p.expectedChecksum) return {code:'match',label:'verified'};
  return {code:'mismatch',label:'MISMATCH'};
}

function renderCards(){
  const grid=document.getElementById('card-grid');
  grid.innerHTML = pis.map((p,i)=>{
    const h=health(p);
    const si=STATE_INFO[p.state];
    const cs=checksumStatus(p);
    const isSel=i===selected;
    return `<div class="pi-card${isSel?' selected':''}" onclick="selectPi(${i})">
      <div class="health-strip" style="background:${h.color}"></div>
      <div class="card-header">
        <span class="card-title">Final ${p.id}</span>
        <span class="health-pill" style="background:${h.color}22;color:${h.color}"><span class="health-dot" style="background:${h.color}"></span>${h.label}</span>
      </div>
      <div class="card-body">
        <div class="state-label" style="color:${si.color}">${si.label}</div>
        <div class="msg-line">${p.statusMsg}</div>
        ${p.alarmMsg?`<div class="msg-line alarm-line">⚑ ${p.alarmMsg}</div>`:''}
        <div class="checksum-row ${cs.code==='match'?'checksum-match':cs.code==='mismatch'?'checksum-mismatch':'checksum-pending'}">
          ${cs.code==='match'?'✓':cs.code==='mismatch'?'✗':'…'} checksum ${cs.label}
        </div>
        <div class="wd-age">watchdog: ${fmtAge(p.watchdogAge)}</div>
        <div class="quick-actions">
          ${h.code==='offline'?`<div class="qbtn" onclick="event.stopPropagation();openLaunchModal(${i})">launch</div>`:
            h.code==='red'?`<div class="qbtn warn" onclick="event.stopPropagation();openRebootModal(${i})">reboot</div>`:
            `<div class="qbtn danger" onclick="event.stopPropagation();openShutdownModal(${i})">shutdown</div>`}
        </div>
      </div>
    </div>`;
  }).join('');
}

function renderDetail(){
  const p=pis[selected];
  const h=health(p);
  const si=STATE_INFO[p.state];
  const cs=checksumStatus(p);
  document.getElementById('detail-title').textContent=`Final ${p.id} — control panel`;
  document.getElementById('detail-name').textContent=`Final ${p.id}`;
  document.getElementById('detail-sub').textContent=`ns=2;s=ANT1.Final${p.id}.Status · pi5-checker-0${p.id}.local`;
  document.getElementById('detail-health').innerHTML=`<span class="health-pill" style="background:${h.color}22;color:${h.color};font-size:10px;padding:4px 10px"><span class="health-dot" style="background:${h.color}"></span>${h.label}</span>`;

  let actions='';
  if(h.code==='offline'){
    actions=`<button class="action-btn primary" onclick="openLaunchModal(${selected})"><i class="ti ti-player-play" aria-hidden="true"></i>Auto-launch / login</button>`;
  } else {
    actions=`<button class="action-btn" onclick="openLaunchModal(${selected})" disabled title="Already running"><i class="ti ti-player-play" aria-hidden="true"></i>Auto-launch / login</button>`;
  }
  if(h.code==='red'){
    actions+=`<button class="action-btn warn" onclick="openRebootModal(${selected})"><i class="ti ti-refresh" aria-hidden="true"></i>Reboot (fires RebootRequest)</button>`;
  }
  actions+=`<button class="action-btn danger" onclick="openShutdownModal(${selected})" ${h.code==='offline'?'disabled':''}><i class="ti ti-power" aria-hidden="true"></i>Shutdown (fires ShutdownRequest)</button>`;
  actions+=`<button class="action-btn" onclick="openBackupModal(${selected})"><i class="ti ti-upload" aria-hidden="true"></i>Backup logs now</button>`;
  document.getElementById('action-bar').innerHTML=actions;

  const watchdogCell=`
    <div class="info-cell">
      <div class="info-title">Watchdog &amp; state</div>
      <div class="kv-row"><span class="kv-key">CheckerState</span><span style="color:${si.color};font-weight:500">${p.state} — ${si.label}</span></div>
      <div class="kv-row"><span class="kv-key">Last watchdog</span><span>${fmtAge(p.watchdogAge)}</span></div>
      <div class="kv-row"><span class="kv-key">Threshold</span><span>10s</span></div>
      <div class="kv-row"><span class="kv-key">SSH reachable</span><span style="color:${p.sshReachable?'#10b981':'#ef4444'}">${p.sshReachable?'yes':'no'}</span></div>
      <div class="kv-row"><span class="kv-key">Last boot</span><span>${p.lastBoot?fmtAge(p.lastBoot===null?null:(Date.now()-new Date(p.lastBoot.replace(' ','T')).getTime())/1000):'unknown'}</span></div>
    </div>`;

  const checksumCell=`
    <div class="info-cell">
      <div class="info-title">Checksum verification</div>
      <div class="kv-row"><span class="kv-key">Expected (MES)</span><span>${p.expectedChecksum}</span></div>
      <div class="kv-row"><span class="kv-key">Actual (reported)</span><span>${p.actualChecksum||'—'}</span></div>
      <div class="checksum-box" style="background:${cs.code==='match'?'#06281f':cs.code==='mismatch'?'#2a0d0d':'#1a1d28'};color:${cs.code==='match'?'#10b981':cs.code==='mismatch'?'#ef4444':'#64748b'}">
        <span>${cs.code==='match'?'Verified at startup':cs.code==='mismatch'?'Mismatch — investigate image':'Awaiting checksum report'}</span>
        ${cs.code==='match'?'<i class="ti ti-check" aria-hidden="true"></i>':cs.code==='mismatch'?'<i class="ti ti-x" aria-hidden="true"></i>':''}
      </div>
    </div>`;

  const logsCell=`
    <div class="info-cell">
      <div class="info-title">Log backup — SUS-PE3DATA-02</div>
      <div class="kv-row"><span class="kv-key">Last sync</span><span>${fmtAge(p.lastBackup===null?null:(Date.now()-new Date(p.lastBackup.replace(' ','T')).getTime())/1000)}</span></div>
      <div class="kv-row"><span class="kv-key">Transfer</span><span>sFTP</span></div>
      <div class="kv-row"><span class="kv-key">Schedule</span><span>every 2h</span></div>
      <div class="log-list" style="margin-top:6px">
        <div class="log-entry"><span>checker_${ts(0).slice(0,10)}.log</span><span class="badge" style="background:#06281f;color:#10b981">synced</span></div>
        <div class="log-entry"><span>checker_${ts(86400).slice(0,10)}.log</span><span class="badge" style="background:#06281f;color:#10b981">synced</span></div>
        <div class="log-entry"><span>checker_${ts(172800).slice(0,10)}.log</span><span class="badge" style="background:#06281f;color:#10b981">synced</span></div>
      </div>
    </div>`;

  const ackCell=`
    <div class="info-cell">
      <div class="info-title">Pending requests</div>
      <div class="kv-row"><span class="kv-key">AutoRequest</span><span class="badge" style="background:#1a2035;color:#64748b">idle</span></div>
      <div class="kv-row"><span class="kv-key">ResetRequest</span><span class="badge" style="background:#1a2035;color:#64748b">idle</span></div>
      <div class="kv-row"><span class="kv-key">ShutdownRequest</span><span class="badge" style="background:#1a2035;color:#64748b">idle</span></div>
      <div class="kv-row"><span class="kv-key">RebootRequest</span><span class="badge" style="background:#1a2035;color:#64748b">idle</span></div>
      <div style="margin-top:8px;font-size:8.5px;color:var(--c-muted);line-height:1.5">Reboot fires <span style="color:#a78bfa">RebootRequest</span>, then this panel watches for <span style="color:#06b6d4">CheckerState=100</span> to confirm.</div>
    </div>`;

  document.getElementById('info-grid').innerHTML = watchdogCell + checksumCell + logsCell + ackCell;
}

function selectPi(i){ selected=i; renderCards(); renderDetail(); }

let pendingAction=null;
function openModal(title,body,confirmLabel,confirmClass,onConfirm){
  document.getElementById('modal-title').innerHTML=title;
  document.getElementById('modal-body').innerHTML=body;
  const btn=document.getElementById('modal-confirm');
  btn.textContent=confirmLabel;
  btn.className='modal-btn '+confirmClass;
  pendingAction=onConfirm;
  document.getElementById('modal-overlay').classList.add('open');
}
function closeModal(){ document.getElementById('modal-overlay').classList.remove('open'); pendingAction=null; }
function confirmModal(){ if(pendingAction) pendingAction(); closeModal(); }

function openLaunchModal(i){
  const p=pis[i];
  openModal(`<i class="ti ti-player-play" aria-hidden="true"></i>Auto-launch Final ${p.id}`,
    `Connects to pi5-checker-0${p.id}.local, logs in as eng, and runs the checker launch script. The reported boot checksum will appear here once the script completes.`,
    'Launch','confirm-warn',()=>{
      p.state=100; p.statusMsg='Starting up — running launch script'; p.watchdogAge=0; p.sshReachable=true;
      renderCards(); renderDetail();
    });
}
function openRebootModal(i){
  const p=pis[i];
  openModal(`<i class="ti ti-refresh" aria-hidden="true"></i>Reboot Final ${p.id}`,
    `This fires <b style="color:#f3f4f6">RebootRequest</b> over OPC. The panel will watch <b style="color:#f3f4f6">CheckerState</b> for code 100 to confirm the checker is coming back up.`,
    'Confirm reboot','confirm-warn',()=>{
      p.state=102; p.statusMsg='Reboot request acknowledged — rebooting'; p.alarmMsg='';
      renderCards(); renderDetail();
    });
}
function openShutdownModal(i){
  const p=pis[i];
  openModal(`<i class="ti ti-power" aria-hidden="true"></i>Shut down Final ${p.id}`,
    `This fires <b style="color:#f3f4f6">ShutdownRequest</b> over OPC. The Pi will power down gracefully; the UPS will not bring it back up automatically unless power was lost.`,
    'Confirm shutdown','confirm-danger',()=>{
      p.state=101; p.statusMsg='Shutdown request acknowledged'; 
      renderCards(); renderDetail();
    });
}
function openBackupModal(i){
  const p=pis[i];
  openModal(`<i class="ti ti-upload" aria-hidden="true"></i>Backup logs — Final ${p.id}`,
    `Runs an sFTP transfer of checker logs to SUS-PE3DATA-02 outside the normal 2-hour schedule.`,
    'Run backup now','confirm-warn',()=>{
      p.lastBackup=ts(0); renderDetail();
    });
}

function updateClock(){
  const d=new Date();
  document.getElementById('clock').textContent=
    d.toLocaleDateString('en-US',{month:'short',day:'numeric'})+' '+
    String(d.getHours()).padStart(2,'0')+':'+String(d.getMinutes()).padStart(2,'0')+':'+String(d.getSeconds()).padStart(2,'0');
}

function simulateTick(){
  pis.forEach(p=>{
    if(p.watchdogAge!==null && p.state!==0) p.watchdogAge+=1;
    if(p.state===100){ p.watchdogAge=0; }
  });
  renderCards();
  renderDetail();
}

renderCards();
renderDetail();
updateClock();
setInterval(updateClock,1000);
setInterval(simulateTick,2000);
</script>

```

## Architecture overview

The existing `OpcUtilities` project already proves out `EasyUAClient` against KepServerEX — this will function more like a library instead of a console app. `CheckerRemoteBridge` becomes the Blazor Server host. Server-side rendering (which is already in place with `InteractiveServerComponents`) is the right call here because Pi credentials, SSH sessions, and OPC subscriptions all need to live on a trusted server, never in browser-side code.

```ps
CheckerRemoteBridge (Blazor Server)
 ├─ Components/Pages/Home.razor          — the dashboard from above (add more components for code reuse as possible)
 └─ Services/
     ├─ OpcMonitorService.cs              — background service, owns EasyUAClient subscriptions
     ├─ CheckerStateStore.cs              — in-memory state, fires events on change
     ├─ PiControlService.cs               — SSH or agent calls (launch/reboot/shutdown/checksum)
     ├─ ChecksumVerificationService.cs    — compares actual vs expected
     └─ LogBackupService.cs               — sFTP/SCP to SUS-PE3DATA-02 (later)

OpcUtilities
 └─ Program.cs                            — wraps EasyUAClient: Read/Write/Subscribe by node ID
```

### 1. Ensure `OpcUtilities` is usable both as console app and service library

Right now `OpcUtilities.Program.Main` owns the `EasyUAClient` as a `static readonly` field and drives everything interactively. This is good and likely supports the necessary registration of OPC work as a service. Create something like the following interface only if necessary:

```csharp
public interface IOpcClient
{
    Task<object> ReadAsync(string nodeId);
    Task WriteAsync(string nodeId, object value);
    IDisposable SubscribeDataChange(string nodeId, int updateRateMs, Action<string, object> onChange);
}
```

### 2. `OpcMonitorService` — the only thing that talks to KepServer

A single `BackgroundService` (hosted service) that, for Status group only, subscribes to these 12 tags per Final (60 total across 5 stations):

```ps
CheckerState, CheckerStatusMessage, CheckerAlarmMessage,
AutoRequest, AutoRequestACK, ResetRequest, ResetRequestACK,
ShutdownRequest, ShutdownRequestACK, RebootRequest, RebootRequestACK,
WatchdogDateTime
```

Use `SubscribeDataChange` per tag (or batch-subscribe if QuickOPC's API supports a list overload — check `EasyUAClient.SubscribeMultipleDataChanges`, which is more efficient than 60 individual subscriptions). On each callback, update a shared `CheckerStateStore` and raise a `.NET` event or `IObservable` that Blazor components subscribe to via `StateHasChanged`.

```csharp
public sealed class CheckerStateStore
{
    private readonly ConcurrentDictionary<int, CheckerStatus> _states = new();
    public event Action<int>? StatusChanged;

    public CheckerStatus Get(int finalId) => _states.GetOrAdd(finalId, _ => new());

    public void Update(int finalId, Action<CheckerStatus> mutate)
    {
        mutate(_states.GetOrAdd(finalId, _ => new()));
        StatusChanged?.Invoke(finalId);
    }
}

public sealed class CheckerStatus
{
    public int CheckerState { get; set; }
    public string StatusMessage { get; set; } = "";
    public string AlarmMessage { get; set; } = "";
    public DateTime? WatchdogDateTime { get; set; }
    public bool AutoRequestACK { get; set; }
    public bool ResetRequestACK { get; set; }
    public bool ShutdownRequestACK { get; set; }
    public bool RebootRequestACK { get; set; }
}
```

The Razor component injects `CheckerStateStore` as a singleton, subscribes to `StatusChanged` in `OnInitialized`, and calls `InvokeAsync(StateHasChanged)` on each tick — standard Blazor Server pattern, no extra SignalR hub needed since Blazor Server already maintains a circuit per browser tab.

### 3. Health derivation (green/red) — compute it, don't store it

This is exactly the "watchdog age" logic in the mockup. Don't try to get KepServer to communicate "red" — derive it client-side (well, server-side, in the Blazor component or a small `HealthEvaluator`):

```csharp
public static HealthState Evaluate(CheckerStatus s, DateTime now)
{
    if (s.CheckerState == 0) return HealthState.Offline;
    if (s.CheckerState is 100 or 101 or 102) return HealthState.Transitioning;
    bool watchdogStale = s.WatchdogDateTime is null || (now - s.WatchdogDateTime.Value) > TimeSpan.FromSeconds(10);
    bool hasAlarm = !string.IsNullOrEmpty(s.AlarmMessage);
    return (watchdogStale || hasAlarm) ? HealthState.Red : HealthState.Green;
}
```

Run this on a `PeriodicTimer` tick (every 1-2s) independent of OPC callbacks, since watchdog staleness needs to advance even when no new OPC data arrives — that's the whole point of a watchdog.

### 4. Reboot/shutdown actions — write to OPC, then watch for confirmation

This part is basically already designed in the tag map. The Razor component (or a `CheckerActionService`) does:

```csharp
public async Task RequestRebootAsync(int finalId)
{
    await _opcClient.WriteAsync($"ns=2;s=ANT1.Final{finalId}.Status.RebootRequest", true);
    // QuickOPC handles the pulse; if not, write true then false after ~1s
}
```

Then the UI just watches `CheckerStateStore` for that station transitioning to `CheckerState == 100`, same as the modal copy says. No polling loop needed beyond what the subscription already provides — the `RebootRequestACK` and subsequent state change will arrive as OPC callbacks naturally.

### 5. The two undecided pieces — sketched both ways

#### Reaching the Pi for auto-launch/SSH and checksum

*Option A — SSH.NET from the server.* Add `SSH.NET` (Renci.SshNet) to `CheckerRemoteBridge`. Store per-Pi host/credentials in configuration (user secrets locally, environment variables or Key Vault in production — never in `appsettings.json` committed to source). Each action becomes:

```csharp
using var client = new SshClient(host, "eng", password); // or key-based auth, preferred
client.Connect();
var cmd = client.RunCommand("/home/eng/launch_checker.sh");
client.Disconnect();
```

Pros: zero new code on the Pi side, shell scripts already in place. Cons: server now holds SSH credentials for 5 machines, and you're shelling out somewhat blindly — `RunCommand` blocks until the script returns, which is fine for "launch in background and exit" scripts (`nohup ./launch.sh &`) but wrong if the script itself runs forever in foreground.

*Option B — small HTTP agent on each Pi.* A minimal `systemd`-managed Flask/ASP.NET/Node service on each Pi exposing `POST /launch`, `POST /checksum`, `GET /health`. The Blazor server calls these over HTTP (ideally on a private VLAN, with a shared secret header).

Pros: no SSH credentials on the server; the agent can do richer things (stream logs, report progress, run checksum and push it to OPC itself rather than round-tripping through the bridge); much easier to reason about exactly what's exposed. Cons: it's a second small codebase to write and deploy to 5 Pis, plus there must be a way to keep the agent itself alive and updated.

Given there are already shell scripts and the Pis aren't going to scale past 5, **Lean toward Option A to start** — it gets software working the fastest, and it can be swapped for an agent later behind the same `IPiControlService` interface without touching the Blazor UI. Worth flagging: if "auto-launch from interface" needs to survive a Pi reboot cycle, the launch script should be triggered via `systemd` (a `oneshot` service or timer) rather than a literal SSH-invoked foreground process, since the SSH session won't survive across the reboot itself.

#### Expected checksum source

*Option A — new OPC tag group.* Add e.g. `Status.ExpectedChecksum` (String) to the tag map, populated by whatever already populates `Recipe.Current` from MES, or set manually per deployment. The bridge just reads it like any other tag — zero new integration code, fits the existing "everything flows through KepServer" pattern, and other OPC clients (including the checker itself, if it wants to self-verify) get the same value for free.

*Option B — direct MES call.* The Blazor server calls a MES REST endpoint or queries a MES database/view directly for the expected checksum per model/line, similar to the `systemRequirements.md` `ModelToLine` pattern. More plumbing (new connection string or HTTP client, auth, error handling for MES being unreachable) but keeps "what should this checksum be" logically owned by MES rather than smuggled through an OPC tag that has nothing to do with the actual checker test sequence.

**Lean toward Option A (OPC tag).** It's consistent with everything else in this system, requires no new external dependency, and the checksum's only real use right now is exactly the display-and-compare feature — it doesn't need MES's full authority, just a value MES (or a person) sets once per deployed image.

### 6. Suggested build order

1. Extract `OpcUtilities` into the wrapper library; confirm `SubscribeDataChange` works from a hosted service (not just console `Main`) — async subscription lifetimes inside `BackgroundService` are the first thing likely to cause issues.
2. Build `CheckerStateStore` + `OpcMonitorService` for Status group only; get the 5-card grid rendering real subscribed state with no actions yet.
3. Add the watchdog-age health evaluator and the green/red pill — this alone closes the "no app monitoring today" gap.
4. Wire `RebootRequest`/`ShutdownRequest` writes with confirmation modals; verify the ACK and state-100 transition show up live.
5. Add SSH-based launch (Option A) for one Pi end-to-end before scripting all 5.
6. Add checksum tag read + compare once the tag exists in KepServer.
7. Add the sFTP/SCP log backup as a separate `TimedHostedService` — keep it decoupled from the OPC monitor entirely, since it has nothing to do with tag state.
