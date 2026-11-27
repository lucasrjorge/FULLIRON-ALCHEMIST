import * as THREE from "three";
import { OrbitControls } from "three/addons/controls/OrbitControls.js";

const $ = (id) => document.getElementById(id);

const STATE_NAMES = [
  "Cru",
  "Incompleto",
  "Dissolvido",
  "Superdissolvido",
  "Calcinado",
  "Supercalcinado",
  "Cinza",
  "Gás Fraco",
  "Gás Estável",
  "Gás Instável",
  "Cristalizado",
  "Resíduo",
  "Queimado",
  "Corrompido"
];

const ELEMENT_GLYPHS = { aqua: "▽", ignis: "△", terra: "▽≡", aer: "△≡" };

const ELEMENTS = {
  aqua: {
    name: "Aqua",
    color: "#7fb7d9",
    description: "Água. Dissolve, umedece e conduz a mistura."
  },
  ignis: {
    name: "Ignis",
    color: "#ff9b52",
    description: "Fogo. Aquece, separa e transforma."
  },
  terra: {
    name: "Terra",
    color: "#b08d57",
    description: "Terra. Fixa, materializa e guarda resíduos."
  },
  aer: {
    name: "Aer",
    color: "#c9d7c9",
    description: "Ar. Volatiliza, eleva e dispersa."
  }
};

const COMPONENTS = {
  sulfur: {
    name: "Enxofre",
    color: "#d8b43a",
    state: 0,
    tags: ["Calor"],
    description: "Representa a alma e a inflamabilidade.",
    primary: true,
    residueWeak: "terra_sulfur_weak",
    residueStrong: "terra_sulfur_strong"
  },
  mercury: {
    name: "Mercúrio",
    color: "#a9b0c0",
    state: 0,
    tags: ["Volátil"],
    description: "Representa o espírito e a volatilidade.",
    primary: true,
    residueWeak: "terra_mercury_weak",
    residueStrong: "terra_mercury_strong"
  },
  salt: {
    name: "Sal",
    color: "#e8e8e8",
    state: 0,
    tags: ["Seco", "Puro"],
    description: "Representa o corpo físico e a estabilidade.",
    primary: true,
    residueWeak: "terra_salt_weak",
    residueStrong: "terra_salt_strong"
  },
  dew: {
    name: "Orvalho",
    color: "#7fb7d9",
    state: 0,
    tags: ["Úmido", "Puro"],
    description: "Meio líquido puro para banhar e umedecer as misturas.",
    primary: true,
    residueWeak: "terra_dew_weak",
    residueStrong: "terra_dew_strong"
  },
  iron: {
    name: "Ferro",
    color: "#8a8f98",
    state: 0,
    tags: ["Seco"],
    description: "Liga-se à força, à ação e à coragem.",
    primary: true,
    residueWeak: "terra_iron_weak",
    residueStrong: "terra_iron_strong"
  },
  sulfur_incomplete: {
    name: "Enxofre Incompleto",
    color: "#c9b06a",
    state: 1,
    tags: ["Úmido", "Seco"],
    description: "Enxofre que passou por uma Dissolutio incompleta."
  },
  sulfur_dissolved: {
    name: "Enxofre Dissolvido",
    color: "#b89b4a",
    state: 2,
    tags: ["Úmido"],
    description: "Enxofre filtrado por Dissolutio."
  },
  sulfur_superdissolved: {
    name: "Enxofre Superdissolvido",
    color: "#8a6a2b",
    state: 3,
    tags: ["Úmido"],
    description: "Enxofre que passou do ponto na Dissolutio."
  },
  sulfur_calcined: {
    name: "Enxofre Calcinado",
    color: "#d19a3a",
    state: 4,
    tags: ["Calor", "Seco"],
    description: "Enxofre que passou por Calcinatio."
  },
  sulfur_supercalcined: {
    name: "Enxofre Supercalcinado",
    color: "#a86b1f",
    state: 5,
    tags: ["Calor", "Seco"],
    description: "Enxofre que passou do ponto na Calcinatio."
  },
  sulfur_ash: {
    name: "Enxofre Cinza",
    color: "#5a5a5a",
    state: 6,
    tags: ["Seco"],
    description: "Enxofre reduzido a cinza."
  },
  sulfur_gas_weak: {
    name: "Enxofre Gás Fraco",
    color: "#e8e0a0",
    state: 7,
    tags: ["Volátil"],
    description: "Gás de Enxofre no estado fraco."
  },
  sulfur_gas_stable: {
    name: "Enxofre Gás Estável",
    color: "#d8b43a",
    state: 8,
    tags: ["Volátil"],
    description: "Gás de Enxofre no estado estável."
  },
  sulfur_gas_unstable: {
    name: "Enxofre Gás Instável",
    color: "#b07c2a",
    state: 9,
    tags: ["Volátil"],
    description: "Gás de Enxofre no estado instável."
  },
  sulfur_crystal_weak: {
    name: "Cristal de Enxofre Fraco",
    color: "#e8d98a",
    state: 10,
    tags: ["Puro"],
    description: "Cristal formado a partir do gás fraco de Enxofre."
  },
  sulfur_crystal_stable: {
    name: "Cristal de Enxofre Estável",
    color: "#f0c15a",
    state: 10,
    tags: ["Puro"],
    description: "Cristal formado a partir do gás estável de Enxofre."
  },
  sulfur_crystal_unstable: {
    name: "Cristal de Enxofre Instável",
    color: "#c49a2a",
    state: 10,
    tags: ["Puro"],
    description: "Cristal formado a partir do gás instável de Enxofre."
  },
  gold: {
    name: "Ouro",
    color: "#f0c15a",
    state: 10,
    tags: ["Puro"],
    description: "Simboliza a perfeição e a pureza.",
    primary: true
  },
  terra_sulfur: { name: "Terra de Enxofre", color: "#8a6a2b", state: 11, tags: ["Seco"], description: "Resíduo de Terra deixado por Enxofre." },
  terra_sulfur_weak: { name: "Terra de Enxofre Fraca", color: "#b89b4a", state: 11, tags: ["Seco"], description: "Resíduo fraco de Calcinatio do Enxofre." },
  terra_sulfur_strong: { name: "Terra de Enxofre Forte", color: "#5a3e12", state: 11, tags: ["Seco"], description: "Resíduo forte de Calcinatio do Enxofre." },
  terra_mercury: { name: "Terra de Mercúrio", color: "#6b6b7a", state: 11, tags: ["Seco"], description: "Resíduo de Terra deixado por Mercúrio." },
  terra_mercury_weak: { name: "Terra de Mercúrio Fraca", color: "#9a9ab0", state: 11, tags: ["Seco"], description: "Resíduo fraco de Calcinatio do Mercúrio." },
  terra_mercury_strong: { name: "Terra de Mercúrio Forte", color: "#3e3e4a", state: 11, tags: ["Seco"], description: "Resíduo forte de Calcinatio do Mercúrio." },
  terra_salt: { name: "Terra de Sal", color: "#d9d9d9", state: 11, tags: ["Seco"], description: "Resíduo de Terra deixado por Sal." },
  terra_salt_weak: { name: "Terra de Sal Fraca", color: "#ededed", state: 11, tags: ["Seco"], description: "Resíduo fraco de Calcinatio do Sal." },
  terra_salt_strong: { name: "Terra de Sal Forte", color: "#afafaf", state: 11, tags: ["Seco"], description: "Resíduo forte de Calcinatio do Sal." },
  terra_dew: { name: "Terra de Orvalho", color: "#4a6b5a", state: 11, tags: ["Seco"], description: "Resíduo de Terra deixado por Orvalho." },
  terra_dew_weak: { name: "Terra de Orvalho Fraca", color: "#6f9a86", state: 11, tags: ["Seco"], description: "Resíduo fraco de Calcinatio do Orvalho." },
  terra_dew_strong: { name: "Terra de Orvalho Forte", color: "#2e4a3d", state: 11, tags: ["Seco"], description: "Resíduo forte de Calcinatio do Orvalho." },
  terra_iron: { name: "Terra de Ferro", color: "#6b4a2b", state: 11, tags: ["Seco"], description: "Resíduo de Terra deixado por Ferro." },
  terra_iron_weak: { name: "Terra de Ferro Fraca", color: "#8c6a45", state: 11, tags: ["Seco"], description: "Resíduo fraco de Calcinatio do Ferro." },
  terra_iron_strong: { name: "Terra de Ferro Forte", color: "#3e2a18", state: 11, tags: ["Seco"], description: "Resíduo forte de Calcinatio do Ferro." }
};

const CALC_OUTCOMES = [
  { min: 0, max: 0.15, out: "sulfur", state: 0, message: "Ainda cru.", residue: false },
  { min: 0.15, max: 0.45, out: "sulfur_calcined", state: 4, message: "Calcinado.", residue: true },
  { min: 0.45, max: 0.75, out: "sulfur_supercalcined", state: 5, message: "Supercalcinado.", residue: true },
  { min: 0.75, max: 1, out: "sulfur_ash", state: 6, message: "Cinza.", residue: true }
];

const RECIPE = {
  id: "ouro",
  name: "Transmutação de Ouro",
  symbol: "Ouro",
  result: "gold",
  resultState: 10,
  description: "O Enxofre Calcinado, o Sal e o Orvalho se unem para formar Ouro.",
  failure: "Os componentes não se combinaram.",
  requirements: [
    { element: "ignis", component: "sulfur_calcined", state: 4 },
    { element: "terra", component: "salt", state: 0 },
    { element: "aqua", component: "dew", state: 0 }
  ]
};

const SAVE_KEY = "fulliron.grimoire.web.v1";

function emptySave() {
  return { components: {}, recipes: {}, lastFailure: null };
}

function loadSave() {
  try {
    const raw = localStorage.getItem(SAVE_KEY);
    if (!raw) return emptySave();
    const parsed = JSON.parse(raw);
    return {
      components: parsed.components && typeof parsed.components === "object" ? parsed.components : {},
      recipes: parsed.recipes && typeof parsed.recipes === "object" ? parsed.recipes : {},
      lastFailure: typeof parsed.lastFailure === "string" ? parsed.lastFailure : null
    };
  } catch (error) {
    return emptySave();
  }
}

let grimoire = loadSave();

function persist() {
  try {
    localStorage.setItem(SAVE_KEY, JSON.stringify(grimoire));
  } catch (error) {
    return;
  }
}

const canvas = $("scene");
const renderer = new THREE.WebGLRenderer({ canvas, antialias: true, powerPreference: "high-performance" });
renderer.setPixelRatio(Math.min(window.devicePixelRatio || 1, 2));
renderer.setSize(window.innerWidth, window.innerHeight);
renderer.outputColorSpace = THREE.SRGBColorSpace;
renderer.toneMapping = THREE.ACESFilmicToneMapping;
renderer.toneMappingExposure = 1.16;
renderer.shadowMap.enabled = true;
renderer.shadowMap.type = THREE.PCFSoftShadowMap;

const scene = new THREE.Scene();
scene.background = new THREE.Color(0x080604);
scene.fog = new THREE.FogExp2(0x080604, 0.032);

const camera = new THREE.PerspectiveCamera(44, window.innerWidth / window.innerHeight, 0.1, 100);
camera.position.set(0, 4.9, 8.8);

const controls = new OrbitControls(camera, canvas);
controls.target.set(0, 1.05, 1.35);
controls.enableDamping = true;
controls.dampingFactor = 0.075;
controls.enablePan = false;
controls.minDistance = 5.2;
controls.maxDistance = 13;
controls.minPolarAngle = Math.PI * 0.19;
controls.maxPolarAngle = Math.PI * 0.475;
controls.update();

const hemi = new THREE.HemisphereLight(0xb9a684, 0x1a120a, 1.08);
scene.add(hemi);

const keyLight = new THREE.DirectionalLight(0xffe7c4, 1.68);
keyLight.position.set(4.5, 9, 6.5);
keyLight.castShadow = true;
keyLight.shadow.mapSize.set(2048, 2048);
keyLight.shadow.camera.near = 1;
keyLight.shadow.camera.far = 30;
keyLight.shadow.camera.left = -8;
keyLight.shadow.camera.right = 8;
keyLight.shadow.camera.top = 9;
keyLight.shadow.camera.bottom = -8;
keyLight.shadow.bias = -0.0004;
scene.add(keyLight);

const fillLight = new THREE.PointLight(0xffb867, 14, 12, 2);
fillLight.position.set(0, 3.1, 3.1);
scene.add(fillLight);

const coolLight = new THREE.PointLight(0x7fa7d9, 7, 12, 2);
coolLight.position.set(-4.6, 2.6, 3.8);
scene.add(coolLight);

const GLOW_TEXTURE = canvasTexture((context, width, height) => {
  const gradient = context.createRadialGradient(width / 2, height / 2, 0, width / 2, height / 2, width / 2);
  gradient.addColorStop(0, "rgba(255,255,255,1)");
  gradient.addColorStop(0.32, "rgba(255,255,255,0.55)");
  gradient.addColorStop(0.7, "rgba(255,255,255,0.12)");
  gradient.addColorStop(1, "rgba(255,255,255,0)");
  context.fillStyle = gradient;
  context.fillRect(0, 0, width, height);
}, 128, 128);

const MAT = {
  floor: new THREE.MeshStandardMaterial({ color: 0x17110c, roughness: 0.96, metalness: 0.05 }),
  wall: new THREE.MeshStandardMaterial({ color: 0x2a2118, roughness: 0.92, metalness: 0.03 }),
  wallDark: new THREE.MeshStandardMaterial({ color: 0x1e1711, roughness: 0.95 }),
  wood: new THREE.MeshStandardMaterial({ color: 0x5a3e2b, roughness: 0.74, metalness: 0.08 }),
  woodDark: new THREE.MeshStandardMaterial({ color: 0x3b2819, roughness: 0.86, metalness: 0.04 }),
  metal: new THREE.MeshStandardMaterial({ color: 0x2a2d31, roughness: 0.42, metalness: 0.86 }),
  brass: new THREE.MeshStandardMaterial({ color: 0xb08d57, roughness: 0.32, metalness: 0.92, emissive: 0x2a1c08, emissiveIntensity: 0.32 }),
  parchment: new THREE.MeshStandardMaterial({ color: 0xd8c39a, roughness: 0.92, metalness: 0.01 }),
  leather: new THREE.MeshStandardMaterial({ color: 0x5a2e1e, roughness: 0.78, metalness: 0.05 }),
  stone: new THREE.MeshStandardMaterial({ color: 0x3a3229, roughness: 0.96, metalness: 0.02 }),
  gold: new THREE.MeshStandardMaterial({ color: 0xf0c15a, roughness: 0.25, metalness: 0.85, emissive: 0x6b4a12, emissiveIntensity: 0.42 }),
  flame: new THREE.MeshBasicMaterial({ color: 0xff8a2a, transparent: true, opacity: 0.85, side: THREE.DoubleSide }),
  flameCore: new THREE.MeshBasicMaterial({ color: 0xffe3a0, transparent: true, opacity: 0.95 }),
  hidden: new THREE.MeshBasicMaterial({ transparent: true, opacity: 0, depthWrite: false, colorWrite: false })
};

const clickTargets = [];
const torches = [];
const shelfRows = [];
const socketOccupant = { calc: null, aqua: null, ignis: null, terra: null, aer: null };
const items = [];

let selectedItem = null;
let activeStation = "lab";
let calcFeedback = "Coloque um Enxofre no cadinho e use o fole.";
let symbolFeedback = "A mesa aguarda os componentes e o desenho do símbolo.";
let calc = { heat: 0, exposure: 0, tracked: null, finished: false };
let uidCounter = 0;
let bannerTimer = null;
let flashTimer = null;
let distortTimer = null;

const SOCKET_POS = {};
let RESIDUE_POS = new THREE.Vector3();
let RESULT_POS = new THREE.Vector3();
const SHELF_SLOT_POS = [];

function componentOf(id) {
  return COMPONENTS[id] || null;
}

function stateName(state) {
  return STATE_NAMES[state] || "—";
}

function componentLabel(item) {
  const comp = componentOf(item.compId);
  return comp ? `${comp.name} · ${stateName(item.state)}` : "Sem componente";
}

function tagsOf(comp) {
  return comp && comp.tags && comp.tags.length ? comp.tags.join(" · ") : "Sem tags";
}

function addBox(parent, w, h, d, x, y, z, material, options = {}) {
  const mesh = new THREE.Mesh(new THREE.BoxGeometry(w, h, d), material);
  mesh.position.set(x, y, z);
  mesh.castShadow = options.castShadow !== false;
  mesh.receiveShadow = options.receiveShadow !== false;
  parent.add(mesh);
  return mesh;
}

function addCylinder(parent, radiusTop, radiusBottom, height, segments, x, y, z, material, options = {}) {
  const mesh = new THREE.Mesh(new THREE.CylinderGeometry(radiusTop, radiusBottom, height, segments), material);
  mesh.position.set(x, y, z);
  mesh.castShadow = options.castShadow !== false;
  mesh.receiveShadow = options.receiveShadow !== false;
  parent.add(mesh);
  return mesh;
}

function addLegs(parent, width, depth, topY, height, material) {
  const lx = width / 2 - 0.08;
  const lz = depth / 2 - 0.08;
  addBox(parent, 0.09, height, 0.09, -lx, topY - height / 2, -lz, material);
  addBox(parent, 0.09, height, 0.09, lx, topY - height / 2, -lz, material);
  addBox(parent, 0.09, height, 0.09, -lx, topY - height / 2, lz, material);
  addBox(parent, 0.09, height, 0.09, lx, topY - height / 2, lz, material);
}

function addClickable(object, data) {
  object.userData = Object.assign({}, object.userData, data);
  clickTargets.push(object);
  return object;
}

function actionOf(object) {
  let node = object;
  while (node) {
    if (node.userData && node.userData.action) return node.userData;
    node = node.parent;
  }
  return null;
}

function canvasTexture(draw, width = 512, height = 128) {
  const surface = document.createElement("canvas");
  surface.width = width;
  surface.height = height;
  const context = surface.getContext("2d");
  draw(context, width, height);
  const texture = new THREE.CanvasTexture(surface);
  texture.colorSpace = THREE.SRGBColorSpace;
  texture.anisotropy = 4;
  return texture;
}

function textSprite(text, color = "#f0d38a", scale = 1) {
  const texture = canvasTexture((context, width, height) => {
    context.clearRect(0, 0, width, height);
    context.font = "700 64px Cinzel, Georgia, serif";
    context.textAlign = "center";
    context.textBaseline = "middle";
    context.shadowColor = color;
    context.shadowBlur = 22;
    context.fillStyle = color;
    context.fillText(text.toUpperCase(), width / 2, height / 2 + 4);
  }, 900, 180);
  const material = new THREE.SpriteMaterial({ map: texture, transparent: true, depthWrite: false });
  const sprite = new THREE.Sprite(material);
  sprite.scale.set(2.35 * scale, 0.47 * scale, 1);
  return sprite;
}

function drawElementGlyph(context, element, size) {
  context.clearRect(0, 0, size, size);
  const color = ELEMENTS[element] ? ELEMENTS[element].color : "#d9b45a";
  const cx = size / 2;
  const top = size * 0.18;
  const bottom = size * 0.82;
  const left = size * 0.18;
  const right = size * 0.82;
  const middle = size * 0.5;
  context.strokeStyle = color;
  context.lineWidth = size * 0.055;
  context.lineJoin = "round";
  context.lineCap = "round";
  context.shadowColor = color;
  context.shadowBlur = size * 0.12;
  context.beginPath();
  if (element === "aqua" || element === "terra") {
    context.moveTo(left, top);
    context.lineTo(right, top);
    context.lineTo(cx, bottom);
  } else {
    context.moveTo(left, bottom);
    context.lineTo(right, bottom);
    context.lineTo(cx, top);
  }
  context.closePath();
  context.stroke();
  if (element === "terra" || element === "aer") {
    context.beginPath();
    context.moveTo(left * 0.55, middle);
    context.lineTo(right + size * 0.05, middle);
    context.stroke();
  }
}

function glyphTexture(element) {
  return canvasTexture((context, width, height) => {
    context.clearRect(0, 0, width, height);
    const inner = Math.min(width, height);
    context.save();
    context.translate((width - inner) / 2, (height - inner) / 2);
    drawElementGlyph(context, element, inner);
    context.restore();
  }, 256, 256);
}

function glyphSprite(element, scale = 0.52) {
  const material = new THREE.SpriteMaterial({ map: glyphTexture(element), transparent: true, depthWrite: false });
  const sprite = new THREE.Sprite(material);
  sprite.scale.set(scale, scale, 1);
  return sprite;
}

function glyphPlane(element, size = 0.34) {
  const material = new THREE.MeshBasicMaterial({
    map: glyphTexture(element),
    transparent: true,
    depthWrite: false,
    side: THREE.DoubleSide
  });
  const mesh = new THREE.Mesh(new THREE.PlaneGeometry(size, size), material);
  mesh.rotation.x = -Math.PI / 2;
  return mesh;
}

function addRing(parent, inner, outer, color, x, y, z, opacity = 0.8, rotationX = -Math.PI / 2) {
  const material = new THREE.MeshBasicMaterial({
    color,
    transparent: true,
    opacity,
    side: THREE.DoubleSide,
    depthWrite: false
  });
  const mesh = new THREE.Mesh(new THREE.RingGeometry(inner, outer, 96), material);
  mesh.position.set(x, y, z);
  mesh.rotation.x = rotationX;
  parent.add(mesh);
  return mesh;
}

function addLineLoop(parent, points, color, opacity = 0.8, y = 0.02) {
  const geometry = new THREE.BufferGeometry().setFromPoints(points);
  const material = new THREE.LineBasicMaterial({ color, transparent: true, opacity });
  const line = new THREE.LineLoop(geometry, material);
  line.position.y = y;
  parent.add(line);
  return line;
}

function buildRoom() {
  const floor = new THREE.Mesh(new THREE.PlaneGeometry(24, 24), MAT.floor);
  floor.rotation.x = -Math.PI / 2;
  floor.position.y = -0.02;
  floor.receiveShadow = true;
  scene.add(floor);

  addBox(scene, 14, 4.4, 0.28, 0, 2.2, -5.1, MAT.wall, { castShadow: false });
  addBox(scene, 0.28, 4.4, 11, -6.9, 2.2, 0.4, MAT.wallDark, { castShadow: false });
  addBox(scene, 0.28, 4.4, 11, 6.9, 2.2, 0.4, MAT.wallDark, { castShadow: false });
  addBox(scene, 14, 0.24, 0.32, 0, 0.12, 5.3, MAT.wallDark, { castShadow: false });

  const wallCircle = new THREE.Group();
  wallCircle.position.set(0, 2.55, -4.95);
  scene.add(wallCircle);
  addRing(wallCircle, 1.16, 1.19, 0xd9b45a, 0, 0, 0, 0.28, 0);
  addRing(wallCircle, 0.82, 0.84, 0xd9b45a, 0, 0, 0, 0.18, 0);
  addRing(wallCircle, 0.3, 0.32, 0xd9b45a, 0, 0, 0, 0.18, 0);
  const triangleGeometry = new THREE.BufferGeometry().setFromPoints([
    new THREE.Vector3(0, 1.02, 0),
    new THREE.Vector3(0.96, -0.36, 0),
    new THREE.Vector3(-0.96, -0.36, 0)
  ]);
  wallCircle.add(new THREE.LineLoop(triangleGeometry, new THREE.LineBasicMaterial({ color: 0xd9b45a, transparent: true, opacity: 0.22 })));

  const glyphPositions = [
    ["ignis", 0, 1.38],
    ["aer", 1.34, 0],
    ["aqua", 0, -1.38],
    ["terra", -1.34, 0]
  ];
  for (const [element, gx, gy] of glyphPositions) {
    const sprite = glyphSprite(element, 0.42);
    sprite.position.set(gx, gy, 0.03);
    wallCircle.add(sprite);
  }

  addRing(scene, 2.35, 2.39, 0xd9b45a, 0, 0.005, 2.0, 0.22);
  addRing(scene, 1.55, 1.57, 0xd9b45a, 0, 0.006, 2.0, 0.14);
  const spokes = [];
  for (let i = 0; i < 12; i += 1) {
    const angle = (i / 12) * Math.PI * 2;
    spokes.push(new THREE.Vector3(Math.cos(angle) * 1.58, 0.012, Math.sin(angle) * 1.58 + 2.0));
  }
  addLineLoop(scene, spokes, 0xd9b45a, 0.12, 0.012);

  addTorch(-6.45, -1.6);
  addTorch(6.45, -1.6);
  addTorch(-6.45, 2.7);

  const dustPositions = new Float32Array(320 * 3);
  for (let i = 0; i < 320; i += 1) {
    dustPositions[i * 3] = (Math.random() - 0.5) * 12;
    dustPositions[i * 3 + 1] = Math.random() * 3.8 + 0.1;
    dustPositions[i * 3 + 2] = (Math.random() - 0.5) * 9 + 1.5;
  }
  const dustGeometry = new THREE.BufferGeometry();
  dustGeometry.setAttribute("position", new THREE.BufferAttribute(dustPositions, 3));
  const dust = new THREE.Points(dustGeometry, new THREE.PointsMaterial({
    color: 0xe2c078,
    size: 0.017,
    transparent: true,
    opacity: 0.5,
    depthWrite: false,
    blending: THREE.AdditiveBlending
  }));
  dust.name = "Dust";
  scene.add(dust);

  addBox(scene, 2.2, 0.02, 1.4, 0, 0.005, 2.2, MAT.leather, { castShadow: false });
}

function addTorch(x, z) {
  const group = new THREE.Group();
  group.position.set(x, 2.15, z);
  scene.add(group);

  addBox(group, 0.16, 0.4, 0.16, 0, -0.28, 0, MAT.metal);
  addCylinder(group, 0.18, 0.11, 0.2, 12, 0, 0, 0, MAT.metal);
  const flame = new THREE.Mesh(new THREE.ConeGeometry(0.1, 0.34, 10), MAT.flame);
  flame.position.y = 0.22;
  group.add(flame);
  const core = new THREE.Mesh(new THREE.ConeGeometry(0.045, 0.2, 8), MAT.flameCore);
  core.position.y = 0.2;
  group.add(core);

  const light = new THREE.PointLight(0xff9b52, 7, 7, 2);
  light.position.set(0, 0.3, 0);
  group.add(light);
  torches.push({ light, flame, core, seed: Math.random() * 10 });
}

function buildShelf() {
  const root = new THREE.Group();
  root.name = "Shelf";
  root.position.set(-4.45, 0, 0.1);
  scene.add(root);

  addBox(root, 0.22, 3.1, 3.6, -0.28, 1.65, 0, MAT.woodDark);
  addBox(root, 0.09, 3.1, 3.6, -0.42, 1.65, 0, MAT.woodDark);

  const rows = [
    { compId: "sulfur", y: 1.04, z: -1.1 },
    { compId: "mercury", y: 1.66, z: -1.1 },
    { compId: "salt", y: 2.28, z: -1.1 },
    { compId: "dew", y: 1.04, z: 0.72 },
    { compId: "iron", y: 1.66, z: 0.72 }
  ];

  rows.forEach((row, rowIndex) => {
    addBox(root, 0.56, 0.08, 1.62, 0, row.y - 0.06, row.z, MAT.wood);
    addBox(root, 0.12, 0.42, 0.12, 0.22, row.y + 0.15, row.z - 0.72, MAT.woodDark);
    addBox(root, 0.12, 0.42, 0.12, 0.22, row.y + 0.15, row.z + 0.72, MAT.woodDark);

    for (let i = 0; i < 3; i += 1) {
      const leather = new THREE.MeshStandardMaterial({
        color: [0x3e2a1c, 0x2e3a2a, 0x3a2438, 0x4a3a24][i % 4],
        roughness: 0.86
      });
      addBox(root, 0.16, 0.3 + i * 0.025, 0.12, -0.15, row.y + 0.16, row.z - 0.55 + i * 0.08, leather);
    }

    const button = addCylinder(root, 0.07, 0.07, 0.05, 16, 0.3, row.y + 0.02, row.z + 0.58, MAT.brass);
    addClickable(button, { action: "restock", row: rowIndex });
    const label = textSprite("REPOR", "#f0d38a", 0.22);
    label.position.set(0.3, row.y + 0.2, row.z + 0.58);
    root.add(label);

    shelfRows.push({
      compId: row.compId,
      y: row.y,
      z: row.z,
      slotLocal: [
        new THREE.Vector3(0.3, row.y + 0.12, row.z - 0.4),
        new THREE.Vector3(0.3, row.y + 0.12, row.z),
        new THREE.Vector3(0.3, row.y + 0.12, row.z + 0.4)
      ],
      root
    });
  });

  const sign = textSprite("ESTANTE", "#b08d57", 0.55);
  sign.position.set(0.1, 3.05, 0);
  root.add(sign);
}

let calcRefs = {};
let symbolRefs = {};

function buildCalcination() {
  const root = new THREE.Group();
  root.name = "Calcination";
  root.position.set(2.75, 0, 1.05);
  scene.add(root);

  addBox(root, 1.9, 0.12, 1.12, 0, 0.9, 0, MAT.wood);
  addLegs(root, 1.9, 1.12, 0.9, 0.9, MAT.woodDark);
  addBox(root, 0.06, 0.56, 0.06, -0.82, 1.16, -0.48, MAT.metal);
  addBox(root, 0.06, 0.56, 0.06, 0.82, 1.16, -0.48, MAT.metal);
  addBox(root, 1.7, 0.05, 0.06, 0, 1.42, -0.48, MAT.metal);

  addCylinder(root, 0.33, 0.27, 0.2, 24, 0, 1.05, 0, MAT.metal);
  addCylinder(root, 0.25, 0.25, 0.03, 24, 0, 1.15, 0, MAT.brass);
  const ember = new THREE.Mesh(new THREE.CylinderGeometry(0.19, 0.19, 0.02, 20), new THREE.MeshBasicMaterial({ color: 0xff7a22 }));
  ember.position.set(0, 1.155, 0);
  root.add(ember);

  const flame = new THREE.Mesh(new THREE.ConeGeometry(0.2, 0.7, 12), MAT.flame);
  flame.position.set(0, 1.48, 0);
  root.add(flame);
  const flameCore = new THREE.Mesh(new THREE.ConeGeometry(0.09, 0.4, 10), MAT.flameCore);
  flameCore.position.set(0, 1.42, 0);
  root.add(flameCore);

  const fireLight = new THREE.PointLight(0xff8a2a, 1.2, 5, 2);
  fireLight.position.set(0, 1.55, 0);
  fireLight.castShadow = false;
  root.add(fireLight);

  const pump = addBox(root, 0.34, 0.08, 0.22, -0.66, 1.0, 0.4, MAT.brass);
  addClickable(pump, { action: "pump" });
  const pumpLabel = textSprite("BOMBEAR", "#ffd98f", 0.28);
  pumpLabel.position.set(-0.66, 1.18, 0.4);
  root.add(pumpLabel);

  const seal = addBox(root, 0.34, 0.08, 0.22, 0.66, 1.0, 0.4, MAT.brass);
  addClickable(seal, { action: "seal" });
  const sealLabel = textSprite("SELAR", "#ffd98f", 0.28);
  sealLabel.position.set(0.66, 1.18, 0.4);
  root.add(sealLabel);

  addBox(root, 0.12, 0.78, 0.12, 0.86, 1.38, -0.28, MAT.metal);
  const heatBar = new THREE.Mesh(new THREE.BoxGeometry(0.055, 1, 0.055), new THREE.MeshBasicMaterial({ color: 0xff9b3d }));
  heatBar.position.set(0.86, 1.05, -0.26);
  root.add(heatBar);
  const gaugeLabel = textSprite("CALOR", "#ffcf87", 0.2);
  gaugeLabel.position.set(0.86, 1.84, -0.26);
  root.add(gaugeLabel);

  addBox(root, 0.76, 0.08, 0.76, -1.3, 0.88, 0.48, MAT.wood);
  addBox(root, 0.6, 0.05, 0.6, -1.3, 0.93, 0.48, MAT.stone);
  const terraGlyph = glyphSprite("terra", 0.32);
  terraGlyph.position.set(-1.3, 1.1, 0.48);
  root.add(terraGlyph);
  const terraLabel = textSprite("TERRA", "#b08d57", 0.2);
  terraLabel.position.set(-1.3, 1.32, 0.48);
  root.add(terraLabel);

  const title = textSprite("CALCINATIO", "#f0d38a", 0.66);
  title.position.set(0, 2.02, 0.1);
  root.add(title);

  const hitbox = addBox(root, 1.9, 0.9, 0.8, 0, 0.5, -0.1, MAT.hidden, { castShadow: false, receiveShadow: false });
  addClickable(hitbox, { action: "station", station: "calcination" });

  const calcSocketHit = addCylinder(root, 0.27, 0.27, 0.11, 20, 0, 1.2, 0, MAT.hidden, { castShadow: false, receiveShadow: false });
  addClickable(calcSocketHit, { action: "socket", socketId: "calc" });

  return { root, fireLight, flame, flameCore, heatBar, ember, calcSocketHit };
}

function buildSymbolTable() {
  const root = new THREE.Group();
  root.name = "SymbolTable";
  root.position.set(0, 0, 2.25);
  scene.add(root);

  addBox(root, 2.0, 0.12, 1.26, 0, 0.9, 0, MAT.wood);
  addLegs(root, 2.0, 1.26, 0.9, 0.9, MAT.woodDark);
  addBox(root, 1.86, 0.03, 1.12, 0, 0.965, 0, MAT.leather, { castShadow: false });

  addRing(root, 0.43, 0.455, 0xd9b45a, 0, 0.982, 0, 0.42);
  addRing(root, 0.22, 0.23, 0xd9b45a, 0, 0.983, 0, 0.24);
  const triangleGeometry = new THREE.BufferGeometry().setFromPoints([
    new THREE.Vector3(-0.62, 0.985, -0.24),
    new THREE.Vector3(0.62, 0.985, -0.24),
    new THREE.Vector3(-0.62, 0.985, 0.26)
  ]);
  root.add(new THREE.LineLoop(triangleGeometry, new THREE.LineBasicMaterial({ color: 0xd9b45a, transparent: true, opacity: 0.34 })));

  const anchors = {
    aqua: new THREE.Vector3(-0.62, 1.08, -0.24),
    ignis: new THREE.Vector3(0.62, 1.08, -0.24),
    terra: new THREE.Vector3(-0.62, 1.08, 0.26),
    aer: new THREE.Vector3(0.62, 1.08, 0.26)
  };

  for (const element of Object.keys(anchors)) {
    const local = anchors[element];
    addCylinder(root, 0.11, 0.13, 0.05, 18, local.x, 0.995, local.z, MAT.brass);
    const flatGlyph = glyphPlane(element, 0.32);
    flatGlyph.position.set(local.x, 1.0, local.z - 0.2);
    root.add(flatGlyph);
    const label = textSprite(ELEMENTS[element].name, ELEMENTS[element].color, 0.18);
    label.position.set(local.x, 1.28, local.z);
    root.add(label);
    const hit = addCylinder(root, 0.17, 0.17, 0.07, 20, local.x, 0.99, local.z, MAT.hidden, { castShadow: false, receiveShadow: false });
    addClickable(hit, { action: "socket", socketId: element });
  }

  addCylinder(root, 0.14, 0.17, 0.06, 18, 0, 1.0, 0, MAT.brass);
  addRing(root, 0.16, 0.18, 0xf0c15a, 0, 1.04, 0, 0.46);

  const drawButton = addBox(root, 0.46, 0.06, 0.2, 0, 1.0, 0.62, MAT.brass);
  addClickable(drawButton, { action: "openSymbol" });
  const drawLabel = textSprite("DESENHAR", "#ffe3a0", 0.26);
  drawLabel.position.set(0, 1.2, 0.62);
  root.add(drawLabel);

  const title = textSprite("MESA DO SÍMBOLO", "#f0d38a", 0.62);
  title.position.set(0, 2.06, 0);
  root.add(title);

  const hitbox = addBox(root, 2.0, 0.88, 0.84, 0, 0.48, -0.12, MAT.hidden, { castShadow: false, receiveShadow: false });
  addClickable(hitbox, { action: "station", station: "symbol" });

  return { root, anchors };
}

function buildGrimoire() {
  const root = new THREE.Group();
  root.name = "Grimoire";
  root.position.set(-3.15, 0, 2.9);
  root.rotation.y = 0.16;
  scene.add(root);

  addBox(root, 1.18, 0.1, 0.78, 0, 0.9, 0, MAT.wood);
  addLegs(root, 1.18, 0.78, 0.9, 0.9, MAT.woodDark);
  addBox(root, 0.92, 0.05, 0.62, 0, 0.955, 0, MAT.leather);
  addBox(root, 0.43, 0.028, 0.56, -0.22, 0.985, 0, MAT.parchment);
  addBox(root, 0.43, 0.028, 0.56, 0.22, 0.985, 0, MAT.parchment);
  addBox(root, 0.03, 0.034, 0.58, 0, 0.99, 0, MAT.leather);

  addCylinder(root, 0.06, 0.07, 0.22, 14, 0.4, 1.0, -0.22, MAT.parchment);
  const candleFlame = new THREE.Mesh(new THREE.ConeGeometry(0.035, 0.12, 8), MAT.flameCore);
  candleFlame.position.set(0.4, 1.17, -0.22);
  root.add(candleFlame);
  const candleLight = new THREE.PointLight(0xffb867, 2.5, 2.6, 2);
  candleLight.position.set(0.4, 1.22, -0.22);
  root.add(candleLight);
  torches.push({ light: candleLight, flame: candleFlame, core: candleFlame, seed: 4.2, small: true });

  const title = textSprite("GRIMÓRIO", "#f0d38a", 0.52);
  title.position.set(0, 1.62, 0);
  root.add(title);

  const hit = addBox(root, 1.02, 0.2, 0.66, 0, 1.02, 0, MAT.hidden, { castShadow: false, receiveShadow: false });
  addClickable(hit, { action: "station", station: "grimoire" });

  return root;
}

function buildWorld() {
  buildRoom();
  buildShelf();
  scene.updateMatrixWorld(true);
  calcRefs = buildCalcination();
  symbolRefs = buildSymbolTable();
  buildGrimoire();
  scene.updateMatrixWorld(true);

  SOCKET_POS.calc = calcRefs.root.localToWorld(new THREE.Vector3(0, 1.46, 0));
  for (const element of Object.keys(symbolRefs.anchors)) {
    SOCKET_POS[element] = symbolRefs.root.localToWorld(symbolRefs.anchors[element].clone());
  }
  RESIDUE_POS = calcRefs.root.localToWorld(new THREE.Vector3(-1.3, 1.02, 0.48));
  RESULT_POS = symbolRefs.root.localToWorld(new THREE.Vector3(0, 1.24, 0));

  shelfRows.forEach((row) => {
    SHELF_SLOT_POS.push(row.slotLocal.map((slot) => row.root.localToWorld(slot.clone())));
  });

  for (let rowIndex = 0; rowIndex < shelfRows.length; rowIndex += 1) {
    for (let slot = 0; slot < 3; slot += 1) {
      const comp = componentOf(shelfRows[rowIndex].compId);
      spawnItem(shelfRows[rowIndex].compId, comp.state, { type: "shelf", row: rowIndex, slot }, SHELF_SLOT_POS[rowIndex][slot]);
    }
  }
}

function makeItemObject(compId, state) {
  const comp = componentOf(compId);
  const color = new THREE.Color(comp ? comp.color : "#ffffff");
  const isGas = state >= 7 && state <= 9;
  const isGold = compId === "gold";

  const group = new THREE.Group();
  const material = new THREE.MeshStandardMaterial({
    color,
    roughness: isGold ? 0.22 : 0.34,
    metalness: isGold ? 0.9 : 0.28,
    emissive: color,
    emissiveIntensity: isGas ? 0.62 : isGold ? 0.4 : 0.1,
    transparent: isGas,
    opacity: isGas ? 0.68 : 1
  });

  const geometry = isGas ? new THREE.SphereGeometry(0.1, 20, 14) : new THREE.IcosahedronGeometry(0.088, 1);
  const core = new THREE.Mesh(geometry, material);
  core.castShadow = true;
  core.receiveShadow = true;
  group.add(core);

  const halo = new THREE.Sprite(new THREE.SpriteMaterial({
    map: GLOW_TEXTURE,
    color,
    transparent: true,
    opacity: isGas ? 0.34 : 0.16,
    depthWrite: false,
    blending: THREE.AdditiveBlending
  }));
  halo.scale.set(0.34, 0.34, 1);
  group.add(halo);

  group.userData.phase = Math.random() * Math.PI * 2;
  return { group, core, halo, material, isGas };
}

function updateItemVisuals(item) {
  const comp = componentOf(item.compId);
  const color = new THREE.Color(comp ? comp.color : "#ffffff");
  const isGas = item.state >= 7 && item.state <= 9;
  const isGold = item.compId === "gold";

  item.material.color.copy(color);
  item.material.emissive.copy(color);
  item.material.emissiveIntensity = isGas ? 0.62 : isGold ? 0.4 : 0.1;
  item.material.transparent = isGas;
  item.material.opacity = isGas ? 0.68 : 1;
  item.material.roughness = isGold ? 0.22 : 0.34;
  item.material.metalness = isGold ? 0.9 : 0.28;
  item.halo.material.color.copy(color);
  item.halo.material.opacity = isGas ? 0.34 : 0.16;
  item.isGas = isGas;
  item.group.scale.setScalar(isGas ? 1.22 : 1);
}

function spawnItem(compId, state, location, position) {
  const comp = componentOf(compId);
  if (!comp) return null;
  const visual = makeItemObject(compId, state);
  const uid = `item-${uidCounter += 1}`;
  const item = {
    uid,
    compId,
    state,
    group: visual.group,
    core: visual.core,
    halo: visual.halo,
    material: visual.material,
    isGas: visual.isGas,
    basePos: position.clone(),
    location: Object.assign({}, location)
  };
  visual.group.position.copy(item.basePos);
  visual.group.userData.action = "item";
  visual.group.userData.uid = uid;
  scene.add(visual.group);
  clickTargets.push(visual.group);
  items.push(item);
  if (location.type === "socket") socketOccupant[location.socketId] = item;
  updateItemVisuals(item);
  return item;
}

function removeItem(item, skipUI = false) {
  if (!item) return;
  if (item.location.type === "socket" && socketOccupant[item.location.socketId] === item) {
    socketOccupant[item.location.socketId] = null;
  }
  if (selectedItem === item) clearSelection(true);
  const index = items.indexOf(item);
  if (index >= 0) items.splice(index, 1);
  const targetIndex = clickTargets.indexOf(item.group);
  if (targetIndex >= 0) clickTargets.splice(targetIndex, 1);
  scene.remove(item.group);
  item.core.geometry.dispose();
  item.material.dispose();
  item.halo.material.dispose();
  if (!skipUI) updateUI();
}

function socketPosition(socketId) {
  const base = SOCKET_POS[socketId];
  return base ? base.clone() : new THREE.Vector3(0, 1, 0);
}

function socketLabel(socketId) {
  if (socketId === "calc") return "cadinho";
  return `âncora ${ELEMENTS[socketId] ? ELEMENTS[socketId].name : socketId}`;
}

function placeItem(item, socketId) {
  if (!item || !Object.prototype.hasOwnProperty.call(socketOccupant, socketId)) return false;
  const occupant = socketOccupant[socketId];
  if (occupant && occupant !== item) {
    log(`${componentOf(occupant.compId).name} já ocupa ${socketLabel(socketId)}.`, "error");
    return false;
  }
  if (item.location.type === "socket") socketOccupant[item.location.socketId] = null;
  socketOccupant[socketId] = item;
  item.location = { type: "socket", socketId };
  item.basePos.copy(socketPosition(socketId));
  if (socketId === "calc" && calc.tracked !== item) {
    calc.tracked = item;
    calc.exposure = 0;
    calc.finished = false;
    calcFeedback = "Enxofre no cadinho. BOMBEAR para aquecer.";
  }
  log(`${componentOf(item.compId).name} colocado em ${socketLabel(socketId)}.`);
  clearSelection(true);
  setActiveStation(socketId === "calc" ? "calcination" : "symbol");
  updateUI();
  return true;
}

function selectItem(item) {
  if (!item) return;
  if (selectedItem === item) {
    clearSelection();
    return;
  }
  selectedItem = item;
  if (item.location.type === "shelf") setActiveStation("shelf");
  else if (item.location.type === "socket" && item.location.socketId === "calc") setActiveStation("calcination");
  else if (item.location.type === "socket") setActiveStation("symbol");
  updateUI();
  updateHint();
}

function clearSelection(skipUI = false) {
  selectedItem = null;
  selectionRing.visible = false;
  if (!skipUI) updateUI();
  updateHint();
}

function restockShelfRow(rowIndex) {
  const row = shelfRows[rowIndex];
  if (!row) return;
  let spawned = 0;
  for (let slot = 0; slot < 3; slot += 1) {
    const occupied = items.some((item) => item.location.type === "shelf" && item.location.row === rowIndex && item.location.slot === slot);
    if (!occupied) {
      const comp = componentOf(row.compId);
      spawnItem(row.compId, comp.state, { type: "shelf", row: rowIndex, slot }, SHELF_SLOT_POS[rowIndex][slot]);
      spawned += 1;
    }
  }
  if (spawned > 0) log(`Fileira de ${componentOf(row.compId).name} reposta (${spawned}).`);
  else log("A fileira já está completa.", "error");
  updateUI();
}

function pumpHeat() {
  calc.heat = Math.min(100, calc.heat + 15);
  updateCalcPanel();
}

function sealCalcination() {
  applyCalcination(false);
}

function applyCalcination(automatic) {
  const item = socketOccupant.calc;
  if (!item) {
    calcFeedback = "Coloque um componente no cadinho.";
    log(calcFeedback, "error");
    return;
  }
  if (calc.finished) return;
  if (item.compId !== "sulfur") {
    calcFeedback = `Sem perfil de Calcinatio para ${componentOf(item.compId).name} nesta fatia.`;
    log(calcFeedback, "error");
    updateCalcPanel();
    return;
  }
  if (calc.exposure < 0.15) {
    calcFeedback = "O componente ainda está cru. Aqueça antes de selar.";
    log(calcFeedback, "error");
    updateCalcPanel();
    return;
  }
  const outcome = CALC_OUTCOMES.find((entry) => calc.exposure >= entry.min && calc.exposure <= entry.max) || CALC_OUTCOMES[CALC_OUTCOMES.length - 1];
  if (outcome.out === item.compId && outcome.state === item.state) {
    calcFeedback = "O componente não mudou de estágio.";
    updateCalcPanel();
    return;
  }

  const strong = calc.heat >= 50;
  const comp = componentOf(item.compId);
  const residueId = strong ? comp.residueStrong : comp.residueWeak;
  item.compId = outcome.out;
  item.state = outcome.state;
  updateItemVisuals(item);
  calc.finished = true;
  discoverComponent(outcome.out);
  calcFeedback = `${automatic ? "Passou do ponto" : "Selado"}: ${componentOf(outcome.out).name}. Retire o componente.`;
  log(`Calcinatio: ${outcome.message} → ${componentOf(outcome.out).name}.`, "success");

  if (outcome.residue && residueId) {
    spawnItem(residueId, 11, { type: "world" }, RESIDUE_POS.clone().add(new THREE.Vector3((Math.random() - 0.5) * 0.16, 0.02, (Math.random() - 0.5) * 0.16)));
    discoverComponent(residueId);
    log(`Calcinatio: resíduo gerado (${componentOf(residueId).name}).`);
  }
  updateCalcPanel();
  updateUI();
}

function calcStageName() {
  if (!socketOccupant.calc) return "Aguardando";
  if (calc.exposure < 0.15) return "Cru";
  if (calc.exposure < 0.45) return "Calcinado";
  if (calc.exposure < 0.75) return "Supercalcinado";
  return "Cinza";
}

function updateCalcPanel() {
  const fill = $("gaugeFill");
  const gaugeValue = $("gaugeValue");
  const stage = $("calcStage");
  const exposure = $("calcExposure");
  const slot = $("calcSlot");
  const feedback = $("calcFeedback");

  if (fill) fill.style.width = `${calc.heat}%`;
  if (gaugeValue) gaugeValue.textContent = `Calor ${Math.round(calc.heat)}`;
  if (stage) stage.textContent = calcStageName();
  if (exposure) exposure.textContent = `Exposição ${Math.round(calc.exposure * 100)}%`;
  if (slot) {
    const item = socketOccupant.calc;
    slot.textContent = item ? componentLabel(item) : "Vazio";
  }
  if (feedback) feedback.textContent = calcFeedback;
}

function updateCalcVisuals(now) {
  if (!calcRefs.root) return;
  const normalized = calc.heat / 100;
  const flicker = 0.9 + Math.sin(now * 0.013) * 0.06 + Math.sin(now * 0.031) * 0.04;
  calcRefs.fireLight.intensity = (0.25 + normalized * 3.4) * flicker;
  const hot = new THREE.Color(0xff5a1a);
  const bright = new THREE.Color(0xffe6a4);
  calcRefs.fireLight.color.copy(hot).lerp(bright, normalized);
  calcRefs.flame.scale.set(0.7 + normalized * 0.7, 0.45 + normalized * 1.05, 0.7 + normalized * 0.7);
  calcRefs.flame.material.opacity = 0.25 + normalized * 0.65;
  calcRefs.flameCore.scale.set(0.7 + normalized * 0.5, 0.45 + normalized * 0.8, 0.7 + normalized * 0.5);
  calcRefs.heatBar.scale.y = Math.max(0.001, normalized);
  calcRefs.heatBar.position.y = 1.02 + (normalized * 0.7) / 2;
  calcRefs.ember.material.color.setRGB(1, 0.35 + normalized * 0.5, 0.1 + normalized * 0.18);
}

function tickCalcination(dt) {
  calc.heat = Math.max(0, calc.heat - 7.5 * dt);
  const item = socketOccupant.calc;
  if (!item) {
    if (calc.tracked) {
      calc.tracked = null;
      calc.exposure = 0;
      calc.finished = false;
    }
    return;
  }
  if (calc.tracked !== item) {
    calc.tracked = item;
    calc.exposure = 0;
    calc.finished = false;
  }
  if (!calc.finished) {
    calc.exposure = Math.min(1, calc.exposure + (calc.heat / 100) * 0.12 * dt);
    if (calc.exposure >= 1) applyCalcination(true);
  }
}

const selectionRing = new THREE.Mesh(
  new THREE.RingGeometry(0.105, 0.14, 40),
  new THREE.MeshBasicMaterial({ color: 0xf0d38a, transparent: true, opacity: 0.85, side: THREE.DoubleSide, depthWrite: false })
);
selectionRing.rotation.x = -Math.PI / 2;
selectionRing.visible = false;
scene.add(selectionRing);

function updateItems(now, dt) {
  for (const item of items) {
    const phase = item.group.userData.phase || 0;
    let bob = 0;
    if (item.location.type === "socket") bob = Math.sin(now * 0.0024 + phase) * 0.008;
    if (selectedItem === item) bob += Math.sin(now * 0.005) * 0.012;
    item.group.position.set(item.basePos.x, item.basePos.y + bob, item.basePos.z);
    item.group.rotation.y += dt * (item.compId === "gold" ? 0.55 : 0.22);
    item.halo.material.opacity = (item.isGas ? 0.28 : 0.13) + Math.sin(now * 0.004 + phase) * 0.04;
  }

  if (selectedItem) {
    selectionRing.visible = true;
    selectionRing.position.set(selectedItem.basePos.x, selectedItem.basePos.y - 0.1, selectedItem.basePos.z);
    selectionRing.scale.setScalar(1 + Math.sin(now * 0.006) * 0.07);
    selectionRing.rotation.z += dt * 0.7;
  } else {
    selectionRing.visible = false;
  }
}

function updateTorches(now) {
  for (const torch of torches) {
    const flicker = 0.82 + Math.sin(now * 0.009 + torch.seed) * 0.1 + Math.sin(now * 0.027 + torch.seed * 2) * 0.05;
    torch.light.intensity = (torch.small ? 2.2 : 7) * flicker;
    const scale = 0.9 + Math.sin(now * 0.012 + torch.seed) * 0.1;
    torch.flame.scale.set(scale, 1.04 + Math.sin(now * 0.017 + torch.seed) * 0.08, scale);
  }
}

function updateDust(dt) {
  const points = scene.getObjectByName("Dust");
  if (!points) return;
  points.rotation.y += dt * 0.012;
  points.position.y = Math.sin(performance.now() * 0.0002) * 0.05;
}

const pointer = new THREE.Vector2();
const raycaster = new THREE.Raycaster();

function pick(event) {
  const rect = canvas.getBoundingClientRect();
  pointer.x = ((event.clientX - rect.left) / rect.width) * 2 - 1;
  pointer.y = -((event.clientY - rect.top) / rect.height) * 2 + 1;
  raycaster.setFromCamera(pointer, camera);
  const hits = raycaster.intersectObjects(clickTargets, true);
  return hits.length ? hits[0] : null;
}

let pointerDown = null;

canvas.addEventListener("pointerdown", (event) => {
  if (event.button !== 0) return;
  pointerDown = { x: event.clientX, y: event.clientY };
  hideTooltip();
});

canvas.addEventListener("pointerup", (event) => {
  if (!pointerDown) return;
  const distance = Math.hypot(event.clientX - pointerDown.x, event.clientY - pointerDown.y);
  pointerDown = null;
  if (distance > 7) return;
  const hit = pick(event);
  if (hit) handleSceneClick(hit);
});

canvas.addEventListener("pointermove", (event) => {
  if (pointerDown) {
    hideTooltip();
    return;
  }
  const hit = pick(event);
  if (!hit) {
    hideTooltip();
    canvas.style.cursor = "grab";
    return;
  }
  const data = actionOf(hit.object);
  if (!data) {
    hideTooltip();
    canvas.style.cursor = "grab";
    return;
  }
  canvas.style.cursor = "pointer";
  showTooltip(data, event.clientX, event.clientY);
});

canvas.addEventListener("pointerleave", () => {
  hideTooltip();
  pointerDown = null;
});

function handleSceneClick(hit) {
  const data = actionOf(hit.object);
  if (!data) return;

  if (data.action === "item") {
    const item = items.find((entry) => entry.uid === data.uid);
    if (item) selectItem(item);
    return;
  }
  if (data.action === "socket") {
    if (selectedItem) {
      placeItem(selectedItem, data.socketId);
    } else {
      const occupant = socketOccupant[data.socketId];
      if (occupant) selectItem(occupant);
      else {
        setActiveStation(data.socketId === "calc" ? "calcination" : "symbol");
        log("Selecione um componente antes de ocupar o socket.", "error");
      }
    }
    return;
  }
  if (data.action === "station") {
    setActiveStation(data.station);
    return;
  }
  if (data.action === "pump") {
    pumpHeat();
    setActiveStation("calcination");
    return;
  }
  if (data.action === "seal") {
    sealCalcination();
    setActiveStation("calcination");
    return;
  }
  if (data.action === "restock") {
    restockShelfRow(data.row);
    return;
  }
  if (data.action === "openSymbol") {
    openDrawing();
    return;
  }
  if (data.action === "openGrimoire") {
    openModal("grimoire");
  }
}

function showTooltip(data, x, y) {
  const tooltip = $("tooltip");
  let html = "";
  if (data.action === "item") {
    const item = items.find((entry) => entry.uid === data.uid);
    if (!item) return;
    const comp = componentOf(item.compId);
    html = `<strong>${comp.name}</strong><small>${stateName(item.state)} · ${tagsOf(comp)}</small><em>${comp.description}</em>`;
  } else if (data.action === "socket") {
    if (data.socketId === "calc") {
      html = "<strong>Cadinho</strong><small>Socket da Calcinatio</small><em>Coloque um componente e pressione BOMBEAR.</em>";
    } else {
      const element = ELEMENTS[data.socketId];
      html = `<strong>${element.name}</strong><small>Âncora do elemento</small><em>${element.description}</em>`;
    }
  } else if (data.action === "station") {
    const names = { calcination: "Calcinatio", symbol: "Mesa do Símbolo", grimoire: "Grimório", shelf: "Estante" };
    html = `<strong>${names[data.station]}</strong><small>Clique para abrir os controles</small>`;
  } else if (data.action === "pump") {
    html = "<strong>BOMBEAR</strong><small>Adiciona calor ao cadinho</small>";
  } else if (data.action === "seal") {
    html = "<strong>SELAR</strong><small>Aplica o estágio atual da Calcinatio</small>";
  } else if (data.action === "restock") {
    html = "<strong>REPOR</strong><small>Reabastece a fileira da estante</small>";
  } else if (data.action === "openSymbol") {
    html = "<strong>DESENHAR</strong><small>Abre a mesa de desenho do símbolo</small>";
  }
  if (!html) {
    hideTooltip();
    return;
  }
  tooltip.innerHTML = html;
  tooltip.classList.remove("hidden");
  const padding = 16;
  const rect = tooltip.getBoundingClientRect();
  let left = x + padding;
  let top = y + padding;
  if (left + rect.width > window.innerWidth - 8) left = x - rect.width - padding;
  if (top + rect.height > window.innerHeight - 8) top = y - rect.height - padding;
  tooltip.style.left = `${Math.max(8, left)}px`;
  tooltip.style.top = `${Math.max(8, top)}px`;
}

function hideTooltip() {
  $("tooltip").classList.add("hidden");
}

function log(message, type = "") {
  const list = $("logList");
  const entry = document.createElement("div");
  entry.className = `log-entry ${type}`.trim();
  entry.textContent = message;
  list.prepend(entry);
  while (list.children.length > 24) list.removeChild(list.lastChild);
}

function setActiveStation(station) {
  activeStation = station;
  renderStationPanel();
  updateHint();
}

function updateHint() {
  const hint = $("hint");
  if (!hint) return;
  if (selectedItem) {
    hint.textContent = `${componentOf(selectedItem.compId).name} selecionado. Clique em um socket ou use os atalhos abaixo.`;
    return;
  }
  const hints = {
    lab: "Selecione um ingrediente na estante à esquerda.",
    shelf: "Clique em uma esfera para pegar um ingrediente. Use REPOR para reabastecer.",
    calcination: "Bombeie para aquecer, observe o estágio e sele quando estiver no ponto.",
    symbol: "Coloque os componentes nas âncoras e abra o desenho do símbolo.",
    grimoire: "Consulte as descobertas e as páginas incompletas das receitas."
  };
  hint.textContent = hints[activeStation] || hints.lab;
}

function renderStationPanel() {
  const name = $("stationName");
  const tag = $("stationTag");
  const body = $("stationBody");
  const names = { lab: "Laboratório", shelf: "Estante", calcination: "Calcinatio", symbol: "Mesa do Símbolo", grimoire: "Grimório" };
  const tags = { lab: "sandbox", shelf: "ingredientes", calcination: "tempo real", symbol: "conjuração", grimoire: "descobertas" };
  name.textContent = names[activeStation] || names.lab;
  tag.textContent = tags[activeStation] || tags.lab;

  if (activeStation === "calcination") {
    const item = socketOccupant.calc;
    body.innerHTML = `
      <p class="panel-desc">A chama decai sozinha. Mantenha o calor com o fole e sele no estágio desejado.</p>
      <div class="slot-card"><span class="slot-label">Cadinho</span><strong id="calcSlot">${item ? componentLabel(item) : "Vazio"}</strong></div>
      <div class="gauge"><div class="gauge-fill" id="gaugeFill"></div><span id="gaugeValue">Calor 0</span></div>
      <div class="stage-row"><span id="calcStage">Aguardando</span><span id="calcExposure">Exposição 0%</span></div>
      <div class="btn-row">
        <button class="btn primary full" data-hold="pump" data-action="pump">BOMBEAR</button>
        <button class="btn full" data-action="seal">SELAR</button>
      </div>
      <div class="feedback-box" id="calcFeedback">${calcFeedback}</div>
      <div class="panel-foot">Estágios: Cru → Calcinado → Supercalcinado → Cinza.<br>Chama forte gera resíduo forte de Terra.</div>
    `;
    updateCalcPanel();
    return;
  }

  if (activeStation === "symbol") {
    body.innerHTML = `
      <p class="panel-desc">Coloque os componentes nas âncoras. O desenho livre é validado na conjuração.</p>
      <div class="anchor-list">
        ${Object.keys(ELEMENTS).map((element) => {
          const occupant = socketOccupant[element];
          const elementData = ELEMENTS[element];
          return `<div class="anchor-row">
            <span class="glyph" style="color:${elementData.color}">${ELEMENT_GLYPHS[element]}</span>
            <span class="occ ${occupant ? "" : "empty"}">${occupant ? componentLabel(occupant) : elementData.name}</span>
            ${selectedItem ? `<button class="mini-btn" data-action="place" data-target="${element}">COLOCAR</button>` : `<span class="mini-btn" style="opacity:.35">—</span>`}
          </div>`;
        }).join("")}
      </div>
      <button class="btn primary full" data-action="openSymbol">ABRIR MESA DE DESENHO</button>
      <div class="feedback-box">${symbolFeedback}</div>
      <div class="panel-foot">Receita demo: Ignis + Enxofre Calcinado, Terra + Sal, Aqua + Orvalho.</div>
    `;
    return;
  }

  if (activeStation === "grimoire") {
    const discovered = Object.keys(grimoire.components).length;
    body.innerHTML = `
      <p class="panel-desc">O Grimório guarda componentes descobertos, a página incompleta da reação e a última falha.</p>
      <div class="slot-card"><span class="slot-label">Componentes</span><strong>${discovered} / ${Object.keys(COMPONENTS).length}</strong></div>
      <div class="slot-card"><span class="slot-label">Reação de Ouro</span><strong>${grimoire.recipes[RECIPE.id] ? "Descoberta" : "Incompleta"}</strong></div>
      ${grimoire.lastFailure ? `<div class="feedback-box" style="border-color:#cf6350">Última falha: ${grimoire.lastFailure}</div>` : ""}
      <button class="btn primary full" data-action="openGrimoire">ABRIR GRIMÓRIO</button>
    `;
    return;
  }

  if (activeStation === "shelf") {
    body.innerHTML = `
      <p class="panel-desc">Ingredientes físicos. Clique em uma esfera para selecionar; o botão REPOR materializa o estoque da fileira.</p>
      <div class="row-list">
        ${shelfRows.map((row, rowIndex) => {
          const comp = componentOf(row.compId);
          const count = items.filter((item) => item.location.type === "shelf" && item.location.row === rowIndex).length;
          return `<div class="row-item">
            <span><span style="display:inline-block;width:8px;height:8px;border-radius:50%;background:${comp.color};margin-right:7px;box-shadow:0 0 8px ${comp.color}"></span>${comp.name}</span>
            <span class="count">${count}/3</span>
            <button class="mini-btn" data-action="restock" data-row="${rowIndex}">REPOR</button>
          </div>`;
        }).join("")}
      </div>
      <div class="panel-foot">Os itens processados continuam sendo objetos físicos: leve-os até a Mesa do Símbolo.</div>
    `;
    return;
  }

  body.innerHTML = `
    <p class="panel-desc">O laboratório está aberto. Escolha um ingrediente na estante ou clique em uma estação.</p>
    <div class="feedback-box">Roteiro: pegue Enxofre, calcine até “Calcinado”, sele, monte a receita e desenhe o símbolo.</div>
    <button class="btn primary full" data-action="openHowto">VER ROTEIRO DEMO</button>
    <button class="btn full" data-action="openGrimoire">ABRIR GRIMÓRIO</button>
  `;
}

function renderTopChips() {
  const compCount = Object.keys(COMPONENTS).length;
  const discovered = Object.keys(grimoire.components).length;
  const recipes = Object.keys(grimoire.recipes).length;
  $("chipDiscoveries").textContent = `Descobertas ${discovered}/${compCount}`;
  $("chipRecipes").textContent = `Reações ${recipes}/1`;
}

function renderSelectionBar() {
  const bar = $("selectionBar");
  if (!selectedItem) {
    bar.classList.add("hidden");
    return;
  }
  const comp = componentOf(selectedItem.compId);
  bar.classList.remove("hidden");
  $("selDot").style.background = comp.color;
  $("selDot").style.boxShadow = `0 0 14px ${comp.color}, inset 0 0 5px rgba(255,255,255,.55)`;
  $("selName").textContent = `${comp.name} · ${stateName(selectedItem.state)}`;
  $("selMeta").textContent = `${tagsOf(comp)} — ${comp.description}`;
  $("selActions").innerHTML = [
    ["calc", "Cadinho"],
    ["aqua", "Aqua"],
    ["ignis", "Ignis"],
    ["terra", "Terra"],
    ["aer", "Aer"]
  ].map(([target, label]) => `<button data-action="place" data-target="${target}">${label}</button>`).join("");
}

function updateUI() {
  renderTopChips();
  renderStationPanel();
  renderSelectionBar();
  if ($("modal-grimoire").classList.contains("open")) renderGrimoire();
}

function discoverComponent(id) {
  if (!COMPONENTS[id] || grimoire.components[id]) return;
  grimoire.components[id] = true;
  persist();
  renderTopChips();
  if ($("modal-grimoire").classList.contains("open")) renderGrimoire();
}

function discoverRecipe(id) {
  if (grimoire.recipes[id]) return;
  grimoire.recipes[id] = true;
  discoverComponent(RECIPE.result);
  persist();
  renderTopChips();
  log(`Grimório: reação descoberta: ${RECIPE.name}.`, "success");
}

function recordFailure(message) {
  grimoire.lastFailure = message;
  persist();
  renderTopChips();
  if ($("modal-grimoire").classList.contains("open")) renderGrimoire();
}

function renderGrimoire() {
  const discoveredRecipe = !!grimoire.recipes[RECIPE.id];
  const result = componentOf(RECIPE.result);
  const requirementHtml = RECIPE.requirements.map((requirement) => {
    const element = ELEMENTS[requirement.element];
    const known = discoveredRecipe || grimoire.components[requirement.component];
    const comp = componentOf(requirement.component);
    return `<div class="req">
      <span class="glyph" style="color:${element.color}">${ELEMENT_GLYPHS[requirement.element]}</span>
      <span class="req-name ${known ? "" : "unknown"}">${known ? comp.name : "???"}</span>
      <span class="req-state">${known ? stateName(requirement.state) : element.name}</span>
    </div>`;
  }).join("");

  const discoveredIds = Object.keys(grimoire.components);
  const discoveryHtml = discoveredIds.length
    ? `<div class="discovery-grid">${discoveredIds.map((id) => {
        const comp = componentOf(id);
        return `<span class="disc"><span class="dot" style="background:${comp.color};box-shadow:0 0 8px ${comp.color}"></span>${comp.name} <small>${stateName(comp.state)}</small></span>`;
      }).join("")}</div>`
    : "<p class=\"empty-note\">Nenhum componente descoberto. Calcine o Enxofre ou conclua a reação.</p>";

  $("grimoireContent").innerHTML = `
    <p class="eyebrow">Grimório do alquimista</p>
    <h2>Páginas de descoberta</h2>
    <div class="grimoire-grid">
      <section class="book-page">
        <h3>${RECIPE.name}</h3>
        <p class="book-sub">${RECIPE.description}</p>
        <span class="recipe-status ${discoveredRecipe ? "done" : ""}">${discoveredRecipe ? "Reação descoberta" : "Página incompleta"}</span>
        <div class="recipe-result">
          <span class="orb" style="background:${result.color};color:${result.color}"></span>
          <div><strong>${result.name}</strong><span>Resultado visível · ${stateName(RECIPE.resultState)}</span></div>
        </div>
        <div class="req-list">${requirementHtml}</div>
        <p class="small">Símbolo: ${discoveredRecipe ? RECIPE.symbol : "???"} · Componentes exatos, estado e posição das âncoras são validados na conjuração.</p>
        ${discoveredRecipe ? "" : "<div class=\"notice\"><strong>Pista de demonstração:</strong> Ignis recebe Enxofre Calcinado; Terra recebe Sal; Aqua recebe Orvalho.</div>"}
        ${grimoire.lastFailure ? `<p class="failure-note">Última falha registrada: ${grimoire.lastFailure}</p>` : ""}
      </section>
      <section class="book-page">
        <h3>Componentes descobertos</h3>
        <p class="book-sub">${discoveredIds.length} de ${Object.keys(COMPONENTS).length} componentes registrados.</p>
        ${discoveryHtml}
        <div class="notice" style="margin-top:16px"><strong>Instruções:</strong> use o fole para manter a chama, sele no estágio desejado e leve o resultado para a Mesa do Símbolo.</div>
        <button class="btn ghost full" id="btnGrimoireReset" style="margin-top:8px">RESETAR DESCOBERTAS</button>
      </section>
    </div>
  `;

  const resetButton = $("btnGrimoireReset");
  if (resetButton) {
    resetButton.addEventListener("click", () => {
      if (!window.confirm("Resetar as descobertas do Grimório?")) return;
      grimoire = emptySave();
      persist();
      renderTopChips();
      renderGrimoire();
      log("Grimório: descobertas resetadas.");
    });
  }
}

function resetLab() {
  if (!window.confirm("Resetar o Grimório e reconstruir o laboratório?")) return;
  grimoire = emptySave();
  persist();
  for (const item of [...items]) removeItem(item, true);
  for (const key of Object.keys(socketOccupant)) socketOccupant[key] = null;
  calc = { heat: 0, exposure: 0, tracked: null, finished: false };
  calcFeedback = "Coloque um Enxofre no cadinho e use o fole.";
  symbolFeedback = "A mesa aguarda os componentes e o desenho do símbolo.";
  selectedItem = null;
  selectionRing.visible = false;
  drawing.strokes = [];
  drawing.current = null;
  for (let rowIndex = 0; rowIndex < shelfRows.length; rowIndex += 1) {
    const comp = componentOf(shelfRows[rowIndex].compId);
    for (let slot = 0; slot < 3; slot += 1) {
      spawnItem(shelfRows[rowIndex].compId, comp.state, { type: "shelf", row: rowIndex, slot }, SHELF_SLOT_POS[rowIndex][slot]);
    }
  }
  document.querySelectorAll(".modal.open").forEach((modal) => modal.classList.remove("open"));
  activeStation = "lab";
  updateUI();
  updateHint();
  log("Laboratório e Grimório resetados.", "success");
}

function showBanner(title, subtitle, type = "") {
  const banner = $("banner");
  banner.innerHTML = `<span class="banner-title">${title}</span><span class="banner-sub">${subtitle}</span>`;
  banner.className = type;
  clearTimeout(bannerTimer);
  bannerTimer = setTimeout(() => {
    banner.className = "hidden";
  }, 3400);
}

function flash(kind) {
  const flashEl = $("flash");
  flashEl.className = "";
  void flashEl.offsetWidth;
  flashEl.className = kind;
  clearTimeout(flashTimer);
  flashTimer = setTimeout(() => {
    flashEl.className = "";
  }, 480);
}

function distortScene() {
  canvas.classList.add("distort");
  clearTimeout(distortTimer);
  distortTimer = setTimeout(() => canvas.classList.remove("distort"), 3200);
}

const drawingCanvas = $("drawingCanvas");
const drawingCtx = drawingCanvas.getContext("2d");
const DRAW_W = drawingCanvas.width;
const DRAW_H = drawingCanvas.height;
const DRAW_ANCHORS = {
  aqua: { x: DRAW_W * 0.3, y: DRAW_H * 0.4 },
  ignis: { x: DRAW_W * 0.7, y: DRAW_H * 0.4 },
  terra: { x: DRAW_W * 0.3, y: DRAW_H * 0.68 },
  aer: { x: DRAW_W * 0.7, y: DRAW_H * 0.68 }
};
const drawing = { strokes: [], current: null, penDown: false, guide: true };

function openModal(id) {
  const modal = $(`modal-${id}`);
  if (!modal) return;
  if (id === "grimoire") renderGrimoire();
  modal.classList.add("open");
  if (id === "drawing") renderDrawing();
}

function closeModal(modal) {
  if (modal) modal.classList.remove("open");
}

function openDrawing() {
  drawing.strokes = [];
  drawing.current = null;
  drawing.penDown = false;
  drawing.guide = true;
  $("btnToggleGuide").textContent = "Ocultar guia";
  renderDrawingChips();
  renderDrawing();
  setDrawingStatus("Desenhe o símbolo principal e marque Aqua, Ignis e Terra.", "");
  openModal("drawing");
}

function closeDrawing() {
  closeModal($("modal-drawing"));
}

function renderDrawingChips() {
  $("drawingChips").innerHTML = Object.keys(ELEMENTS).map((element) => {
    const occupant = socketOccupant[element];
    const comp = occupant ? componentOf(occupant.compId) : null;
    const color = comp ? comp.color : "#3a3229";
    return `<span class="p-chip"><span class="dot" style="background:${color};box-shadow:0 0 7px ${color}"></span>${ELEMENTS[element].name}: <b>${comp ? comp.name : "vazio"}</b></span>`;
  }).join("");
}

function setDrawingStatus(text, type) {
  const element = $("drawingStatus");
  element.textContent = text;
  element.className = `drawing-status ${type || ""}`.trim();
}

function drawingPoint(event) {
  const rect = drawingCanvas.getBoundingClientRect();
  return {
    x: ((event.clientX - rect.left) / rect.width) * DRAW_W,
    y: ((event.clientY - rect.top) / rect.height) * DRAW_H
  };
}

function renderDrawing() {
  const context = drawingCtx;
  context.clearRect(0, 0, DRAW_W, DRAW_H);

  const cx = DRAW_W / 2;
  const cy = DRAW_H * 0.54;
  const radius = 228;

  if (drawing.guide) {
    context.save();
    context.strokeStyle = "rgba(217,180,90,0.22)";
    context.lineWidth = 2;
    context.setLineDash([7, 10]);
    context.beginPath();
    context.arc(cx, cy, radius, 0, Math.PI * 2);
    context.stroke();
    context.setLineDash([]);

    context.strokeStyle = "rgba(217,180,90,0.2)";
    context.beginPath();
    context.moveTo(DRAW_ANCHORS.aqua.x, DRAW_ANCHORS.aqua.y);
    context.lineTo(DRAW_ANCHORS.ignis.x, DRAW_ANCHORS.ignis.y);
    context.lineTo(DRAW_ANCHORS.terra.x, DRAW_ANCHORS.terra.y);
    context.closePath();
    context.stroke();

    context.strokeStyle = "rgba(240,211,138,0.26)";
    context.lineWidth = 3;
    context.beginPath();
    context.arc(cx, cy, 44, 0, Math.PI * 2);
    context.stroke();
    context.beginPath();
    context.arc(cx, cy, 10, 0, Math.PI * 2);
    context.stroke();
    for (let i = 0; i < 12; i += 1) {
      const angle = (i / 12) * Math.PI * 2;
      context.beginPath();
      context.moveTo(cx + Math.cos(angle) * 58, cy + Math.sin(angle) * 58);
      context.lineTo(cx + Math.cos(angle) * 82, cy + Math.sin(angle) * 82);
      context.stroke();
    }
    context.restore();
  }

  context.save();
  context.strokeStyle = "#f0d38a";
  context.lineWidth = 5;
  context.lineCap = "round";
  context.lineJoin = "round";
  context.shadowColor = "#f0c15a";
  context.shadowBlur = 14;
  for (const stroke of drawing.strokes) {
    if (!stroke || stroke.length === 0) continue;
    context.beginPath();
    context.moveTo(stroke[0].x, stroke[0].y);
    for (let i = 1; i < stroke.length; i += 1) context.lineTo(stroke[i].x, stroke[i].y);
    if (stroke.length === 1) context.lineTo(stroke[0].x + 0.5, stroke[0].y + 0.5);
    context.stroke();
  }
  context.restore();

  for (const element of Object.keys(DRAW_ANCHORS)) {
    const anchor = DRAW_ANCHORS[element];
    const color = ELEMENTS[element].color;
    context.save();
    context.strokeStyle = color;
    context.globalAlpha = 0.62;
    context.lineWidth = 1.6;
    context.setLineDash([4, 5]);
    context.beginPath();
    context.arc(anchor.x, anchor.y, 36, 0, Math.PI * 2);
    context.stroke();
    context.setLineDash([]);
    context.globalAlpha = 0.9;
    context.fillStyle = color;
    context.font = "600 19px Inter, system-ui, sans-serif";
    context.textAlign = "center";
    context.textBaseline = "middle";
    const labelY = element === "aqua" || element === "ignis" ? anchor.y - 54 : anchor.y + 54;
    context.fillText(ELEMENTS[element].name.toUpperCase(), anchor.x, labelY);
    context.restore();
  }
}

function pointsOfDrawing() {
  const points = [];
  for (const stroke of drawing.strokes) {
    for (const point of stroke) points.push(point);
  }
  return points;
}

function drawingLength() {
  let length = 0;
  for (const stroke of drawing.strokes) {
    for (let i = 1; i < stroke.length; i += 1) {
      length += Math.hypot(stroke[i].x - stroke[i - 1].x, stroke[i].y - stroke[i - 1].y);
    }
  }
  return length;
}

function minDistance(point, points) {
  let min = Infinity;
  for (const other of points) {
    const distance = Math.hypot(point.x - other.x, point.y - other.y);
    if (distance < min) min = distance;
  }
  return min;
}

function edgeCovered(start, end, points) {
  let covered = 0;
  const samples = 9;
  for (let i = 1; i < samples; i += 1) {
    const t = i / samples;
    const sample = { x: start.x + (end.x - start.x) * t, y: start.y + (end.y - start.y) * t };
    if (minDistance(sample, points) < 48) covered += 1;
  }
  return covered >= 5;
}

function validateDrawing() {
  const points = pointsOfDrawing();
  if (points.length < 12) {
    return { ok: false, reason: "O traço está vazio demais. Desenhe o símbolo principal." };
  }
  const required = ["aqua", "ignis", "terra"];
  const missing = required.filter((element) => minDistance(DRAW_ANCHORS[element], points) >= 64);
  if (missing.length) {
    return { ok: false, reason: `Marque o sigilo de ${missing.map((element) => ELEMENTS[element].name).join(", ")}.` };
  }
  const edges = [
    ["aqua", "ignis"],
    ["ignis", "terra"],
    ["terra", "aqua"]
  ];
  const covered = edges.filter(([a, b]) => edgeCovered(DRAW_ANCHORS[a], DRAW_ANCHORS[b], points)).length;
  if (covered < 2) {
    return { ok: false, reason: "O símbolo principal não conecta as âncoras de forma congruente." };
  }
  if (drawingLength() < 520) {
    return { ok: false, reason: "O traço está fraco demais: a magia não se concentra." };
  }
  return { ok: true, reason: `Símbolo coerente: ${covered}/3 conexões.` };
}

function shakeDrawing() {
  const wrap = document.querySelector(".canvas-wrap");
  wrap.classList.remove("shake");
  void wrap.offsetWidth;
  wrap.classList.add("shake");
  setTimeout(() => wrap.classList.remove("shake"), 420);
}

function castReaction() {
  const validation = validateDrawing();
  if (!validation.ok) {
    setDrawingStatus(validation.reason, "bad");
    shakeDrawing();
    log(`Mesa do Símbolo: ${validation.reason}`, "magic");
    return;
  }

  let success = true;
  for (const requirement of RECIPE.requirements) {
    const item = socketOccupant[requirement.element];
    if (!item || item.compId !== requirement.component || item.state !== requirement.state) {
      success = false;
      break;
    }
  }

  if (success) {
    const consumed = Object.keys(ELEMENTS).map((element) => socketOccupant[element]).filter(Boolean);
    for (const item of consumed) removeItem(item, true);
    spawnItem(RECIPE.result, RECIPE.resultState, { type: "result" }, RESULT_POS.clone());
    discoverRecipe(RECIPE.id);
    discoverComponent(RECIPE.result);
    symbolFeedback = "Reação concluída: Ouro. A página do Grimório foi completada.";
    closeDrawing();
    setActiveStation("symbol");
    updateUI();
    flash("gold");
    showBanner("TRANSMUTAÇÃO DE OURO", RECIPE.description, "");
    log("Mesa do Símbolo: reação concluída. Ouro gerado e registrado no Grimório.", "success");
    return;
  }

  recordFailure(RECIPE.failure);
  symbolFeedback = RECIPE.failure;
  closeDrawing();
  setActiveStation("symbol");
  updateUI();
  flash("violet");
  distortScene();
  showBanner("A REAÇÃO FALHOU", RECIPE.failure, "error");
  log(`Mesa do Símbolo: ${RECIPE.failure}`, "error");
}

drawingCanvas.addEventListener("pointerdown", (event) => {
  event.preventDefault();
  drawingCanvas.setPointerCapture(event.pointerId);
  drawing.penDown = true;
  const point = drawingPoint(event);
  drawing.current = [point];
  drawing.strokes.push(drawing.current);
  renderDrawing();
});

drawingCanvas.addEventListener("pointermove", (event) => {
  if (!drawing.penDown || !drawing.current) return;
  const point = drawingPoint(event);
  const last = drawing.current[drawing.current.length - 1];
  if (Math.hypot(point.x - last.x, point.y - last.y) > 2.2) {
    drawing.current.push(point);
    renderDrawing();
  }
});

function endStroke(event) {
  if (!drawing.penDown) return;
  drawing.penDown = false;
  drawing.current = null;
  if (event && drawingCanvas.hasPointerCapture(event.pointerId)) drawingCanvas.releasePointerCapture(event.pointerId);
  renderDrawing();
}

drawingCanvas.addEventListener("pointerup", endStroke);
drawingCanvas.addEventListener("pointercancel", endStroke);

$("btnClearDrawing").addEventListener("click", () => {
  drawing.strokes = [];
  drawing.current = null;
  drawing.penDown = false;
  renderDrawing();
  setDrawingStatus("Mesa limpa. Desenhe novamente.", "");
});

$("btnToggleGuide").addEventListener("click", () => {
  drawing.guide = !drawing.guide;
  $("btnToggleGuide").textContent = drawing.guide ? "Ocultar guia" : "Mostrar guia";
  renderDrawing();
});

$("btnCast").addEventListener("click", castReaction);
$("closeDrawing").addEventListener("click", closeDrawing);

$("stationBody").addEventListener("click", (event) => {
  const button = event.target.closest("[data-action]");
  if (!button) return;
  const action = button.dataset.action;
  if (action === "seal") {
    sealCalcination();
    setActiveStation("calcination");
  } else if (action === "openSymbol") {
    openDrawing();
  } else if (action === "openGrimoire") {
    openModal("grimoire");
  } else if (action === "openHowto") {
    openModal("howto");
  } else if (action === "restock") {
    restockShelfRow(Number(button.dataset.row));
  } else if (action === "place") {
    if (selectedItem) placeItem(selectedItem, button.dataset.target);
  }
});

let pumpHold = null;
$("stationBody").addEventListener("pointerdown", (event) => {
  const button = event.target.closest("[data-hold=\"pump\"]");
  if (!button) return;
  event.preventDefault();
  pumpHeat();
  clearInterval(pumpHold);
  pumpHold = setInterval(pumpHeat, 170);
});
window.addEventListener("pointerup", () => {
  clearInterval(pumpHold);
  pumpHold = null;
});
window.addEventListener("pointercancel", () => {
  clearInterval(pumpHold);
  pumpHold = null;
});

$("selectionBar").addEventListener("click", (event) => {
  const button = event.target.closest("[data-action=\"place\"]");
  if (!button || !selectedItem) return;
  placeItem(selectedItem, button.dataset.target);
});

$("selClear").addEventListener("click", () => clearSelection());
$("btnReset").addEventListener("click", resetLab);

document.addEventListener("click", (event) => {
  const opener = event.target.closest("[data-open]");
  if (opener) {
    event.preventDefault();
    openModal(opener.dataset.open);
    return;
  }
  const closer = event.target.closest("[data-close]");
  if (closer) {
    event.preventDefault();
    closeModal(closer.closest(".modal"));
  }
});

document.querySelectorAll(".modal").forEach((modal) => {
  modal.addEventListener("click", (event) => {
    if (event.target === modal) closeModal(modal);
  });
});

document.addEventListener("keydown", (event) => {
  if (event.key !== "Escape") return;
  if ($("modal-drawing").classList.contains("open")) {
    closeDrawing();
    return;
  }
  const openModals = [...document.querySelectorAll(".modal.open")];
  if (openModals.length) openModals[openModals.length - 1].classList.remove("open");
});

window.addEventListener("resize", () => {
  camera.aspect = window.innerWidth / window.innerHeight;
  camera.updateProjectionMatrix();
  renderer.setSize(window.innerWidth, window.innerHeight);
});

let previousTime = performance.now();
function animate(now) {
  requestAnimationFrame(animate);
  const dt = Math.min(0.2, (now - previousTime) / 1000);
  previousTime = now;

  controls.update();
  tickCalcination(dt);
  updateItems(now, dt);
  updateTorches(now);
  updateDust(dt);
  updateCalcVisuals(now);
  if (activeStation === "calcination") updateCalcPanel();
  renderer.render(scene, camera);
}

buildWorld();
renderTopChips();
renderStationPanel();
renderSelectionBar();
renderDrawing();
updateHint();
log("Laboratório iniciado. Selecione um ingrediente na estante à esquerda.");
log("Roteiro demo: Enxofre → Calcinatio → Mesa do Símbolo → Ouro.");
requestAnimationFrame(animate);

