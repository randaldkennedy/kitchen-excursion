const grid = document.querySelector('#recipeGrid');
const template = document.querySelector('#recipeCardTemplate');
const searchInput = document.querySelector('#searchInput');
const filtersContainer = document.querySelector('#filters');
const recipeCount = document.querySelector('#recipeCount');
const recipeDialog = document.querySelector('#recipeDialog');
const dialogContent = document.querySelector('#dialogContent');
const cookingDialog = document.querySelector('#cookingDialog');
const cookingTitle = document.querySelector('#cookingTitle');
const cookingSteps = document.querySelector('#cookingSteps');
const shoppingDialog = document.querySelector('#shoppingDialog');
const shoppingContent = document.querySelector('#shoppingContent');
const cookLogDialog = document.querySelector('#cookLogDialog');
const filterToggle = document.querySelector('#filterToggle');
const cookLogForm = document.querySelector('#cookLogForm');
const cookLogNote = document.querySelector('#cookLogNote');
const closeRecipeDialog = document.querySelector('#closeRecipeDialog');
const addRecipeButton = document.querySelector('#addRecipeButton');
const importRecipeButton = document.querySelector('#importRecipeButton');
const recipeImportDialog = document.querySelector('#recipeImportDialog');
const recipeImportForm = document.querySelector('#recipeImportForm');
const recipeImportFile = document.querySelector('#recipeImportFile');
const recipeImportUrl = document.querySelector('#recipeImportUrl');
const recipeImportText = document.querySelector('#recipeImportText');
const recipeImportStatus = document.querySelector('#recipeImportStatus');
const runRecipeImport = document.querySelector('#runRecipeImport');
const closeRecipeImport = document.querySelector('#closeRecipeImport');
const cancelRecipeImport = document.querySelector('#cancelRecipeImport');
const recipeImportModes = [...document.querySelectorAll('[data-import-mode]')];
const recipeImportPanels = [...document.querySelectorAll('[data-import-panel]')];
const recipeEditorDialog = document.querySelector('#recipeEditorDialog');
const recipeEditorForm = document.querySelector('#recipeEditorForm');
const recipeTitle = document.querySelector('#recipeTitle');
const recipeSlug = document.querySelector('#recipeSlug');
const recipeEditorStatus = document.querySelector('#recipeEditorStatus');
const recipeEditorTitle = document.querySelector('#recipeEditorTitle');
const recipeEditorIntro = document.querySelector('#recipeEditorIntro');
const recipeChangeNoteField = document.querySelector('#recipeChangeNoteField');
const closeRecipeEditor = document.querySelector('#closeRecipeEditor');
const cancelRecipeEditor = document.querySelector('#cancelRecipeEditor');
const saveRecipeButton = document.querySelector('#saveRecipeButton');
const accountButton = document.querySelector('#accountButton');
const accountMenu = document.querySelector('#accountMenu');
const accountName = document.querySelector('#accountName');
const accountInitial = document.querySelector('#accountInitial');
const accountMenuName = document.querySelector('#accountMenuName');
const accountEmail = document.querySelector('#accountEmail');
const recipeHeroPhoto = document.querySelector('#recipeHeroPhoto');
const recipeHeroPhotoName = document.querySelector('#recipeHeroPhotoName');
const recipeHeroPreviewWrap = document.querySelector('#recipeHeroPreviewWrap');
const recipeHeroPreview = document.querySelector('#recipeHeroPreview');
const recipeEditorTabs = [...document.querySelectorAll('[data-recipe-tab]')];
const recipeEditorPanels = [...document.querySelectorAll('[data-recipe-panel]')];
const groceryResultDialog = document.querySelector('#groceryResultDialog');
const groceryResultTitle = document.querySelector('#groceryResultTitle');
const groceryResultMessage = document.querySelector('#groceryResultMessage');
const closeGroceryResult = document.querySelector('#closeGroceryResult');
const groceryResultOk = document.querySelector('#groceryResultOk');

const API = '/api';
const GROCERY_API =
  window.location.hostname === 'localhost'
    ? 'http://localhost:5037/api'
    : 'https://grocery.laultimaexcursion.com/api';

let activeCookLogRecipe = null;
let recipePageScrollY = 0;
let recipeHeroPreviewUrl = null;
let editingRecipe = null;
let pendingRecipeSource = null;


filterToggle.addEventListener('click', () => {
  const isExpanded = filterToggle.getAttribute('aria-expanded') === 'true';

  filterToggle.setAttribute('aria-expanded', String(!isExpanded));
  filtersContainer.classList.toggle('filters--collapsed', isExpanded);
  /*filterToggle.textContent = isExpanded ? '▶ Filters' : '▼ Filters'; */
  updateFilterToggleLabel();
});


let recipes = [];
let kitchenUserIsAuthenticated = false;
let kitchenCurrentUser = null;
const activeFilters = {
  meal: 'all',
  protein: 'all',
  method: 'all',
  status: 'all'
};

const filterGroups = [
  { key: 'meal', label: 'Meal', values: ['breakfast', 'lunch', 'dinner', 'side', 'dessert'] },
  { key: 'protein', label: 'Protein', values: ['pork', 'beef', 'chicken', 'turkey','seafood', 'meatless'] },
  { key: 'method', label: 'Method', values: ['oven', 'stovetop', 'slow-cooker', 'grill', 'air-fryer', 'microwave'] },
  { key: 'status', label: 'Status', values: ['favorite', 'la-jefa-approved', 'maybe', 'quick'] }
];

const filterLabels = {
  all: 'All',
  breakfast: 'Breakfast',
  lunch: 'Lunch',
  dinner: 'Dinner',
  side: 'Sides',
  dessert: 'Dessert',
  pork: 'Pork',
  beef: 'Beef',
  chicken: 'Chicken',
  turkey: 'Turkey',
  seafood: 'Seafood',
  meatless: 'Meatless',
  oven: 'Oven',
  stovetop: 'Stovetop',
  'slow-cooker': 'Slow Cooker',
  grill: 'Grill',
  'air-fryer': 'Air Fryer',
  microwave: 'Microwave',
  favorite: 'Favorites',
  'la-jefa-approved': 'La Jefa Approved',
  maybe: 'Maybe',
  quick: 'Quick'
};

function showAnonymousAccountState() {
  kitchenUserIsAuthenticated = false;
  kitchenCurrentUser = null;

  if (accountName) accountName.textContent = 'Sign in';
  if (accountInitial) accountInitial.textContent = '→';
  if (accountMenu) accountMenu.hidden = true;
  accountButton?.setAttribute('aria-expanded', 'false');
  accountButton?.setAttribute('aria-label', 'Sign in to Kitchen Excursion');

  if (addRecipeButton) addRecipeButton.hidden = true;
  if (importRecipeButton) importRecipeButton.hidden = true;
}

function showAuthenticatedAccountState(user) {
  kitchenUserIsAuthenticated = true;
  kitchenCurrentUser = user;

  const displayName = user.givenName || user.email || 'Account';
  const initial = user.givenName?.trim()?.charAt(0)?.toUpperCase() || '?';

  if (accountName) accountName.textContent = displayName;
  if (accountInitial) accountInitial.textContent = initial;
  if (accountMenuName) accountMenuName.textContent = displayName;
  if (accountEmail) accountEmail.textContent = user.email || '';
  accountButton?.setAttribute('aria-label', 'Open account menu');

  if (addRecipeButton) addRecipeButton.hidden = false;
  if (importRecipeButton) importRecipeButton.hidden = false;
}

async function loadAuthenticatedUser() {
  try {
    const statusResponse = await fetch(`${API}/auth/status`);

    if (!statusResponse.ok) {
      showAnonymousAccountState();
      return false;
    }

    const status = await statusResponse.json();
    if (!status.isAuthenticated) {
      showAnonymousAccountState();
      return false;
    }

    const response = await fetch(`${API}/auth/me`);
    if (!response.ok) {
      showAnonymousAccountState();
      return false;
    }

    const user = await response.json();
    if (!user.isAuthenticated) {
      showAnonymousAccountState();
      return false;
    }

    showAuthenticatedAccountState(user);
    return true;
  } catch (error) {
    console.error('Unable to load authenticated user.', error);
    showAnonymousAccountState();
    return false;
  }
}

function requireKitchenSignIn() {
  if (kitchenUserIsAuthenticated) return true;
  window.location.assign(`${API}/auth/login`);
  return false;
}

async function loadRecipes() {
  try {
    const response = await fetch(`${API}/recipes`);
    if (!response.ok) throw new Error(`HTTP ${response.status}`);
    recipes = await response.json();
    buildFilters();
    render();
  } catch (error) {
    grid.innerHTML = `
      <div class="empty">
        <strong>The recipes could not be loaded.</strong><br>
        Open this site through GitHub Pages, IIS, or another local web server rather than directly from the file system.
      </div>`;
    console.error(error);
  }
}

function buildFilters() {
  filtersContainer.innerHTML = '';

  filterGroups.forEach(group => {
    const available = group.values.filter(value =>
      recipes.some(recipe => {
        if (group.key === 'status') {
          return Array.isArray(recipe.status) && recipe.status.includes(value);
        }

        return recipe[group.key] === value;
      })
    );

    if (!available.length) return;

    const section = document.createElement('section');
    section.className = 'filter-group';
    section.setAttribute('aria-label', `${group.label} filters`);

    const heading = document.createElement('span');
    heading.className = 'filter-group__label';
    heading.textContent = group.label;
    section.appendChild(heading);

    const buttonRow = document.createElement('div');
    buttonRow.className = 'filter-group__buttons';

    ['all', ...available].forEach(value => {
      const button = document.createElement('button');
      button.type = 'button';
      button.className =
        `filter${activeFilters[group.key] === value ? ' active' : ''}`;

      button.dataset.group = group.key;
      button.dataset.filter = value;
      button.textContent =
        value === 'all'
          ? group.key === 'meal'
            ? 'All Meals'
            : `All ${group.label}`
          : filterLabels[value] || value;

      button.addEventListener('click', () => {
        activeFilters[group.key] = value;

        buttonRow.querySelectorAll('.filter').forEach(btn => {
          btn.classList.toggle('active', btn === button);
        });

        render();
        updateFilterToggleLabel();
      });

      buttonRow.appendChild(button);
    });

    section.appendChild(buttonRow);
    filtersContainer.appendChild(section);
  })
  const clearButton = document.createElement('button');
  clearButton.type = 'button';
  clearButton.className = 'secondary-btn clear-filters';
  clearButton.textContent = 'Clear All Filters';

  clearButton.addEventListener('click', () => {
    Object.keys(activeFilters).forEach(key => {
      activeFilters[key] = 'all';
    });

    searchInput.value = '';

    buildFilters();
    render();
    updateFilterToggleLabel();
  });

  filtersContainer.appendChild(clearButton);
;
}

function recipeMatchesFilters(recipe) {
  return Object.entries(activeFilters).every(([key, value]) => {
    if (value === 'all') return true;

    if (key === 'status') {
      return Array.isArray(recipe.status) && recipe.status.includes(value);
    }

    return recipe[key] === value;
  });
}

function filteredRecipes() {
  const query = searchInput.value.trim().toLowerCase();
  return recipes.filter(recipe => {
    const haystack = [
      recipe.title,
      recipe.summary,
      recipe.categories.join(' '),
      recipe.ingredients.join(' '),
      recipe.journal?.general || '',
      recipe.meal,
      recipe.protein,
      recipe.method,
      ...(recipe.status || [])
    ].join(' ').toLowerCase();
    return recipeMatchesFilters(recipe) && haystack.includes(query);
  });
}

function updateFilterToggleLabel() {
  const activeCount = Object.values(activeFilters)
    .filter(value => value !== 'all')
    .length;

  const isExpanded = filterToggle.getAttribute('aria-expanded') === 'true';
  const arrow = isExpanded ? '▼' : '▶';
  const count = activeCount ? ` (${activeCount})` : '';

  filterToggle.textContent = `${arrow} Filters${count}`;
}

function starText(value) {
  const stars = Number(value);
  if (!Number.isFinite(stars)) return '';

  const rounded = Math.max(0, Math.min(5, Math.round(stars)));
  return `${'★'.repeat(rounded)}${'☆'.repeat(5 - rounded)}`;
}

function recipeRatingHtml(recipe) {
  if (recipe.rating === null || recipe.rating === undefined) return '';

  return `
    <div class="recipe-rating" aria-label="Average rating ${recipe.rating} out of 5">
      <span class="recipe-rating__stars">${starText(recipe.rating)}</span>
      <strong>${Number(recipe.rating).toFixed(1)}</strong>
    </div>
  `;
}

function recalculateRecipeRating(recipe) {
  const ratings = (recipe.journal?.cookLog || [])
    .flatMap(entry => entry.ratings || [])
    .map(rating => Number(rating.stars))
    .filter(Number.isFinite);

  recipe.rating = ratings.length
    ? Math.round((ratings.reduce((sum, stars) => sum + stars, 0) / ratings.length) * 10) / 10
    : null;
}

function handleUnauthorizedResponse(response) {
  if (response.status !== 401) return false;
  showAnonymousAccountState();
  window.location.assign(`${API}/auth/login`);
  return true;
}

async function saveCookRating(recipe, cookLogId, rater, stars) {
  const response = await fetch(
    `${API}/recipes/${encodeURIComponent(recipe.id)}/cook-log/${cookLogId}/ratings/${encodeURIComponent(rater)}`,
    {
      method: 'PUT',
      headers: { 'Content-Type': 'application/json' },
      body: JSON.stringify({ stars })
    }
  );

  if (handleUnauthorizedResponse(response)) return;

  if (!response.ok) {
    throw new Error(`HTTP ${response.status}`);
  }

  const savedRating = await response.json();
  const entry = recipe.journal?.cookLog?.find(item => Number(item.id) === Number(cookLogId));

  if (entry) {
    entry.ratings ??= [];
    const existing = entry.ratings.find(
      rating => rating.rater.toLowerCase() === savedRating.rater.toLowerCase()
    );

    if (existing) {
      existing.stars = savedRating.stars;
      existing.rater = savedRating.rater;
    } else {
      entry.ratings.push(savedRating);
    }
  }

  recalculateRecipeRating(recipe);
}

function slugifyRecipeTitle(value) {
  return value
    .toLowerCase()
    .trim()
    .normalize('NFD')
    .replace(/[\u0300-\u036f]/g, '')
    .replace(/[^a-z0-9]+/g, '-')
    .replace(/^-+|-+$/g, '')
    .slice(0, 160);
}

function linesToArray(value) {
  return value
    .split(/\r?\n/)
    .map(item => item.trim())
    .filter(Boolean);
}

function commaListToArray(value) {
  return value
    .split(',')
    .map(item => item.trim())
    .filter(Boolean);
}


function setRecipeEditorTab(tabName, focusTab = false) {
  recipeEditorTabs.forEach(tab => {
    const isActive = tab.dataset.recipeTab === tabName;
    tab.classList.toggle('active', isActive);
    tab.setAttribute('aria-selected', String(isActive));

    if (isActive && focusTab) {
      tab.focus();
    }
  });

  recipeEditorPanels.forEach(panel => {
    const isActive = panel.dataset.recipePanel === tabName;
    panel.classList.toggle('active', isActive);
    panel.hidden = !isActive;
  });

  const content = recipeEditorDialog.querySelector('.recipe-editor-content');
  if (content) content.scrollTop = 0;
}

function resetRecipeHeroPhoto(currentImage = null) {
  if (recipeHeroPreviewUrl) {
    URL.revokeObjectURL(recipeHeroPreviewUrl);
    recipeHeroPreviewUrl = null;
  }

  if (recipeHeroPhoto) recipeHeroPhoto.value = '';

  if (currentImage) {
    if (recipeHeroPhotoName) recipeHeroPhotoName.textContent = 'Current photo';
    if (recipeHeroPreview) recipeHeroPreview.src = currentImage;
    if (recipeHeroPreviewWrap) recipeHeroPreviewWrap.hidden = false;
  } else {
    if (recipeHeroPhotoName) recipeHeroPhotoName.textContent = 'No photo selected';
    if (recipeHeroPreview) recipeHeroPreview.removeAttribute('src');
    if (recipeHeroPreviewWrap) recipeHeroPreviewWrap.hidden = true;
  }
}

function updateRecipeHeroPreview() {
  if (!recipeHeroPhoto) return;

  const file = recipeHeroPhoto.files?.[0];

  if (recipeHeroPreviewUrl) {
    URL.revokeObjectURL(recipeHeroPreviewUrl);
    recipeHeroPreviewUrl = null;
  }

  if (!file) {
    if (recipeHeroPhotoName) recipeHeroPhotoName.textContent = 'No photo selected';
    if (recipeHeroPreview) recipeHeroPreview.removeAttribute('src');
    if (recipeHeroPreviewWrap) recipeHeroPreviewWrap.hidden = true;
    return;
  }

  const allowedTypes = new Set(['image/jpeg', 'image/png', 'image/webp']);
  const maxBytes = 15 * 1024 * 1024;

  if (!allowedTypes.has(file.type)) {
    recipeHeroPhoto.value = '';
    recipeEditorStatus.textContent = 'Hero photo must be a JPG, PNG, or WebP image.';
    updateRecipeHeroPreview();
    return;
  }

  if (file.size > maxBytes) {
    recipeHeroPhoto.value = '';
    recipeEditorStatus.textContent = 'Hero photo must be 15 MB or smaller.';
    updateRecipeHeroPreview();
    return;
  }

  recipeEditorStatus.textContent = '';
  recipeEditorStatus.classList.remove('recipe-editor-message--review');
  if (recipeHeroPhotoName) recipeHeroPhotoName.textContent = file.name;

  recipeHeroPreviewUrl = URL.createObjectURL(file);
  if (recipeHeroPreview) recipeHeroPreview.src = recipeHeroPreviewUrl;
  if (recipeHeroPreviewWrap) recipeHeroPreviewWrap.hidden = false;
}

function setFieldValue(name, value) {
  const field = recipeEditorForm.elements[name];
  if (field) field.value = value ?? '';
}

function showGroceryResultDialog(title, message) {
  if (!groceryResultDialog) return;

  groceryResultTitle.textContent = title;
  groceryResultMessage.textContent = message;

  if (!groceryResultDialog.open) {
    groceryResultDialog.showModal();
  }

  groceryResultOk?.focus();
}

function closeGroceryResultDialog() {
  groceryResultDialog?.close();
}

function openRecipeEditor(recipe = null) {
  if (!requireKitchenSignIn()) return;
  if (recipe && !recipe.canEdit) return;

  editingRecipe = recipe;
  pendingRecipeSource = null;
  recipeEditorForm.reset();
  recipeEditorStatus.textContent = '';

  const cookLogTab = recipeEditorTabs.find(tab => tab.dataset.recipeTab === 'cooklog');
  const cookLogPanel = recipeEditorPanels.find(panel => panel.dataset.recipePanel === 'cooklog');

  if (recipe) {
    recipeEditorTitle.textContent = 'Edit Recipe';
    recipeEditorIntro.textContent =
      'Update the recipe and save a new revision. The previous version stays in history.';
    saveRecipeButton.textContent = 'Save Revision';
    recipeChangeNoteField.hidden = false;
    recipeChangeNoteField.style.display = '';
    if (cookLogTab) cookLogTab.hidden = true;
    if (cookLogPanel) cookLogPanel.hidden = true;

    setFieldValue('title', recipe.title);
    setFieldValue('id', recipe.id);
    setFieldValue('meal', recipe.meal);
    setFieldValue('protein', recipe.protein);
    setFieldValue('method', recipe.method);
    setFieldValue('badge', recipe.badge);
    setFieldValue('categories', (recipe.categories || []).join(', '));
    setFieldValue('prep', recipe.prep);
    setFieldValue('cook', recipe.cook);
    setFieldValue('serves', recipe.serves);
    setFieldValue('imageAlt', recipe.imageAlt);
    setFieldValue('summary', recipe.summary);
    setFieldValue('ingredients', (recipe.ingredients || []).join('\n'));
    setFieldValue('steps', (recipe.steps || []).join('\n\n'));
    setFieldValue('generalNotes', recipe.journal?.general || '');
    setFieldValue('changeNote', '');

    recipeEditorForm.querySelectorAll('input[name="status"]').forEach(input => {
      input.checked = (recipe.status || []).includes(input.value);
    });

    recipeSlug.dataset.userEdited = 'true';
    resetRecipeHeroPhoto(recipe.image);
  } else {
    recipeEditorTitle.textContent = 'Add Recipe';
    recipeEditorIntro.textContent =
      'Enter the recipe here and save it straight to Kitchen. Ingredients and instructions are one item per line.';
    saveRecipeButton.textContent = 'Save Recipe';
    recipeChangeNoteField.hidden = true;
    recipeChangeNoteField.style.display = 'none';
    if (cookLogTab) cookLogTab.hidden = false;
    if (cookLogPanel) cookLogPanel.hidden = false;

    recipeEditorForm.elements.rater.value = kitchenCurrentUser?.givenName || 'Randy';
    recipeSlug.dataset.userEdited = 'false';
    resetRecipeHeroPhoto();
  }

  setRecipeEditorTab('details');
  recipeEditorDialog.showModal();
  recipeTitle.focus();
}


function setRecipeImportMode(mode) {
  recipeImportModes.forEach(button => {
    const active = button.dataset.importMode === mode;
    button.classList.toggle('active', active);
    button.setAttribute('aria-selected', String(active));
  });

  recipeImportPanels.forEach(panel => {
    panel.hidden = panel.dataset.importPanel !== mode;
  });

  recipeImportStatus.textContent = '';
}

function openRecipeImport() {
  if (!requireKitchenSignIn()) return;

  recipeImportForm.reset();
  recipeImportStatus.textContent = '';
  runRecipeImport.disabled = false;
  runRecipeImport.textContent = 'Import Draft';
  setRecipeImportMode('file');
  recipeImportDialog.showModal();
}

function applyImportedRecipeDraft(draft, sourceFile) {
  openRecipeEditor();
  pendingRecipeSource = sourceFile;

  recipeEditorIntro.textContent =
    'Imported recipe draft. Review the details, ingredients, and instructions before saving.';
  recipeChangeNoteField.hidden = true;
  recipeChangeNoteField.style.display = 'none';

  setFieldValue('title', draft.title);
  setFieldValue('id', slugifyRecipeTitle(draft.title || ''));
  setFieldValue('meal', draft.meal);
  setFieldValue('protein', draft.protein);
  setFieldValue('method', draft.method);
  setFieldValue('badge', '');
  setFieldValue('categories', (draft.categories || []).join(', '));
  setFieldValue('prep', draft.prep);
  setFieldValue('cook', draft.cook);
  setFieldValue('serves', draft.serves);
  setFieldValue('imageAlt', '');
  setFieldValue('summary', draft.summary);
  setFieldValue('ingredients', (draft.ingredients || []).join('\n'));
  setFieldValue('steps', (draft.steps || []).join('\n\n'));
  setFieldValue('generalNotes', draft.generalNotes || '');

  recipeSlug.dataset.userEdited = 'false';

  const warnings = draft.warnings || [];
  recipeEditorStatus.classList.add('recipe-editor-message--review');
  recipeEditorStatus.textContent = warnings.length
    ? `Imported draft — review before saving. ${warnings.join(' • ')}`
    : 'Imported draft — review it before saving.';

  setRecipeEditorTab('details');
}

async function importRecipeDraft(event) {
  event.preventDefault();

  const activeMode =
    recipeImportModes.find(button => button.classList.contains('active'))
      ?.dataset.importMode || 'file';

  let url;
  let options;
  let sourceFile;

  if (activeMode === 'file') {
    const file = recipeImportFile.files?.[0];

    if (!file) {
      recipeImportStatus.textContent = 'Choose a recipe photo or PDF first.';
      return;
    }

    const form = new FormData();
    form.append('file', file);
    sourceFile = file;

    url = `${API}/recipe-import/file`;
    options = {
      method: 'POST',
      body: form
    };
  } else if (activeMode === 'url') {
    const recipeUrl = recipeImportUrl.value.trim();

    if (!recipeUrl) {
      recipeImportStatus.textContent = 'Enter a recipe website first.';
      return;
    }

    sourceFile = new File(
      [recipeUrl],
      'recipe-source-url.txt',
      { type: 'text/uri-list' }
    );

    url = `${API}/recipe-import/url`;
    options = {
      method: 'POST',
      headers: { 'Content-Type': 'application/json' },
      body: JSON.stringify({ url: recipeUrl })
    };
  } else {
    const text = recipeImportText.value.trim();

    if (!text) {
      recipeImportStatus.textContent = 'Paste some recipe text first.';
      return;
    }

    sourceFile = new File(
      [text],
      'pasted-recipe.txt',
      { type: 'text/plain' }
    );

    url = `${API}/recipe-import/text`;
    options = {
      method: 'POST',
      headers: { 'Content-Type': 'application/json' },
      body: JSON.stringify({ text })
    };
  }

  runRecipeImport.disabled = true;
  runRecipeImport.textContent = 'Reading recipe…';
  recipeImportStatus.textContent =
    activeMode === 'url'
      ? 'Loading and reading the recipe page…'
      : 'Reading the recipe…';

  try {
    const response = await fetch(url, options);

    if (handleUnauthorizedResponse(response)) return;

    if (!response.ok) {
      let message = `HTTP ${response.status}`;

      try {
        const body = await response.json();
        message = body.message || body.error || message;
      } catch {}

      throw new Error(message);
    }

    const draft = await response.json();

    recipeImportDialog.close();

    requestAnimationFrame(() => {
      applyImportedRecipeDraft(draft, sourceFile);
    });
  } catch (error) {
    console.error(error);
    recipeImportStatus.textContent =
      error.message || 'Kitchen could not import that recipe.';
  } finally {
    runRecipeImport.disabled = false;
    runRecipeImport.textContent = 'Import Draft';
  }
}

async function saveRecipe(event) {
  event.preventDefault();

  const form = new FormData(recipeEditorForm);
  const id = String(form.get('id') || '').trim();
  const title = String(form.get('title') || '').trim();

  if (!id || !title) {
    setRecipeEditorTab('details');
    recipeEditorStatus.textContent = 'Title and recipe ID are required.';
    (title ? recipeSlug : recipeTitle).focus();
    return;
  }

  const payload = {
    id,
    title,
    categories: commaListToArray(String(form.get('categories') || '')),
    meal: String(form.get('meal') || '') || null,
    protein: String(form.get('protein') || '') || null,
    method: String(form.get('method') || '') || null,
    status: form.getAll('status'),
    badge: String(form.get('badge') || '') || null,
    imageAlt: String(form.get('imageAlt') || '') || null,
    summary: String(form.get('summary') || '') || null,
    prep: String(form.get('prep') || '') || null,
    cook: String(form.get('cook') || '') || null,
    serves: String(form.get('serves') || '') || null,
    ingredients: linesToArray(String(form.get('ingredients') || '')),
    steps: linesToArray(String(form.get('steps') || '')),
    journal: {
      general: String(form.get('generalNotes') || '') || null
    },
    changeNote: editingRecipe
      ? String(form.get('changeNote') || '').trim() || null
      : null
  };

  const heroPhotoFile = recipeHeroPhoto?.files?.[0] ?? null;
  const originalId = editingRecipe?.id ?? null;

  saveRecipeButton.disabled = true;
  saveRecipeButton.textContent = editingRecipe ? 'Saving revision…' : 'Saving…';
  recipeEditorStatus.textContent = '';

  try {
    const recipeResponse = await fetch(
      editingRecipe
        ? `${API}/recipes/${encodeURIComponent(originalId)}`
        : `${API}/recipes`,
      {
        method: editingRecipe ? 'PUT' : 'POST',
        headers: { 'Content-Type': 'application/json' },
        body: JSON.stringify(payload)
      }
    );

    if (handleUnauthorizedResponse(recipeResponse)) return;

    if (!recipeResponse.ok) {
      let message = `HTTP ${recipeResponse.status}`;
      try {
        const body = await recipeResponse.json();
        message = body.message || message;
      } catch {}
      throw new Error(message);
    }

    const savedRecipe = await recipeResponse.json();

    if (heroPhotoFile) {
      recipeEditorStatus.textContent = 'Uploading photo…';

      const photoForm = new FormData();
      photoForm.append('file', heroPhotoFile);

      const photoResponse = await fetch(
        `${API}/recipes/${encodeURIComponent(savedRecipe.id)}/hero-photo`,
        {
          method: 'POST',
          body: photoForm
        }
      );

      if (handleUnauthorizedResponse(photoResponse)) return;

      if (!photoResponse.ok) {
        let photoMessage = `HTTP ${photoResponse.status}`;
        try {
          const body = await photoResponse.json();
          photoMessage = body.message || photoMessage;
        } catch {}

        throw new Error(
          `${editingRecipe ? 'Revision' : 'Recipe'} saved, but the hero photo failed to upload: ${photoMessage}`
        );
      }
    }

    if (!editingRecipe && pendingRecipeSource) {
      recipeEditorStatus.textContent = 'Saving original recipe source…';

      const sourceForm = new FormData();
      sourceForm.append('file', pendingRecipeSource);

      const sourceResponse = await fetch(
        `${API}/recipes/${encodeURIComponent(savedRecipe.id)}/source`,
        {
          method: 'POST',
          body: sourceForm
        }
      );

      if (handleUnauthorizedResponse(sourceResponse)) return;

      if (!sourceResponse.ok) {
        let sourceMessage = `HTTP ${sourceResponse.status}`;

        try {
          const body = await sourceResponse.json();
          sourceMessage = body.message || sourceMessage;
        } catch {}

        throw new Error(
          `Recipe saved, but the original recipe source failed to upload: ${sourceMessage}`
        );
      }
    }

    if (!editingRecipe) {
      const cookLogNote = String(form.get('cookLogNote') || '').trim();
      const rater = String(form.get('rater') || '').trim();
      const starsValue = String(form.get('stars') || '').trim();

      if (cookLogNote) {
        const ratings = starsValue && rater
          ? [{ rater, stars: Number(starsValue) }]
          : [];

        const cookResponse = await fetch(
          `${API}/recipes/${encodeURIComponent(savedRecipe.id)}/cook-log`,
          {
            method: 'POST',
            headers: { 'Content-Type': 'application/json' },
            body: JSON.stringify({
              author: rater || kitchenCurrentUser?.givenName || 'Cook',
              note: cookLogNote,
              ratings
            })
          }
        );

        if (handleUnauthorizedResponse(cookResponse)) return;

        if (!cookResponse.ok) {
          throw new Error(
            `Recipe saved, but the first cook log failed (HTTP ${cookResponse.status}).`
          );
        }
      }
    }

    const refreshedResponse = await fetch(
      `${API}/recipes/${encodeURIComponent(savedRecipe.id)}`
    );

    if (!refreshedResponse.ok) {
      throw new Error(
        `${editingRecipe ? 'Revision' : 'Recipe'} saved, but reload failed (HTTP ${refreshedResponse.status}).`
      );
    }

    const refreshed = await refreshedResponse.json();

    if (editingRecipe) {
      const index = recipes.findIndex(recipe => recipe.id === originalId);
      if (index >= 0) recipes[index] = refreshed;
      else recipes.push(refreshed);
    } else {
      recipes.push(refreshed);
    }

    buildFilters();
    render();

    editingRecipe = null;
    pendingRecipeSource = null;
    recipeEditorDialog.close();
    openRecipe(refreshed);
  } catch (error) {
    console.error(error);
    recipeEditorStatus.textContent =
      error.message || 'The recipe could not be saved.';
  } finally {
    saveRecipeButton.disabled = false;
    saveRecipeButton.textContent = editingRecipe ? 'Save Revision' : 'Save Recipe';
  }
}


function render() {
  const filtered = filteredRecipes();
  recipeCount.textContent = `${filtered.length} recipe${filtered.length === 1 ? '' : 's'}`;
  grid.innerHTML = '';
  if (!filtered.length) {
    grid.innerHTML = '<p class="empty">No recipes match those filters yet.</p>';
    return;
  }

  filtered.forEach(recipe => {
    const node = template.content.cloneNode(true);
    const img = node.querySelector('.recipe-image');
    if (recipe.image) {
      img.src = recipe.image;
      img.alt = recipe.imageAlt;
    } else {
      img.classList.add('placeholder');
      img.alt = '';
    }
    node.querySelector('.badge').textContent = recipe.badge;
    node.querySelector('.recipe-meta').textContent = `${recipe.prep} prep • ${recipe.cook} cook • serves ${recipe.serves}`;
    const ratingWrap = document.createElement('div');
    ratingWrap.innerHTML = recipeRatingHtml(recipe);
    const cardRating = ratingWrap.firstElementChild;
    if (cardRating) {
      node.querySelector('.recipe-meta').insertAdjacentElement('afterend', cardRating);
    }
    node.querySelector('.recipe-title').textContent = recipe.title;
    img.style.cursor = 'pointer';
    img.addEventListener('click', () => openRecipe(recipe));
    node.querySelector('.recipe-summary').textContent = recipe.summary;
    const tags = node.querySelector('.tag-row');
    recipe.categories.forEach(category => {
      const tag = document.createElement('span');
      tag.className = 'tag';
      tag.textContent = category;
      tags.appendChild(tag);
    });
    node.querySelector('.view-btn').addEventListener('click', () => openRecipe(recipe));
    node.querySelector('.cook-btn').addEventListener('click', () => openCooking(recipe));
    node.querySelector('.shop-btn').addEventListener('click', () => openShopping(recipe));
    grid.appendChild(node);
  });
}

function openRecipe(recipe) {
  const recipeNotes = recipe.journal?.general?.trim() || '';
  const cookLog = recipe.journal?.cookLog || [];

  const cookLogEntriesHtml = cookLog.length
    ? cookLog.map(entry => {
        const cookedAt = new Date(entry.date);

        const formattedDate = cookedAt.toLocaleDateString(undefined, {
          year: 'numeric',
          month: 'long',
          day: 'numeric'
        });

        const formattedTime = cookedAt.toLocaleTimeString(undefined, {
          hour: 'numeric',
          minute: '2-digit'
        });

        const ratings = entry.ratings || [];
        const ratingsHtml = ratings.length
          ? `
              <div class="cook-ratings">
                ${ratings.map(rating => `
                  <span class="cook-rating" title="${rating.stars} out of 5">
                    <strong>${rating.rater}</strong>
                    <span class="cook-rating__stars">${starText(rating.stars)}</span>
                    <span>${rating.stars}</span>
                  </span>
                `).join('')}
              </div>
            `
          : '<p class="cook-rating__empty">No ratings yet.</p>';

        return `
          <article class="cook-log__entry">
            <div class="cook-log__meta">
              <strong>${formattedDate} • ${formattedTime}</strong>
              <div class="cook-log__author">
                by ${entry.author}
              </div>
            </div>

            <p>${entry.note.replace(/\s+/g, ' ').trim()}</p>

            ${ratingsHtml}

            ${kitchenUserIsAuthenticated ? `
            <form class="cook-rating-form" data-cook-log-id="${entry.id}">
              <label>
                <span>Name</span>
                <input name="rater" type="text" value="Randy" required>
              </label>

              <label>
                <span>Stars</span>
                <select name="stars" required>
                  <option value="5">★★★★★ — 5</option>
                  <option value="4">★★★★☆ — 4</option>
                  <option value="3">★★★☆☆ — 3</option>
                  <option value="2">★★☆☆☆ — 2</option>
                  <option value="1">★☆☆☆☆ — 1</option>
                </select>
              </label>

              <button class="secondary-btn" type="submit">Save rating</button>
            </form>
            ` : ''}
          </article>
        `;
      }).join('')
    : '<p class="empty">No cook log entries yet.</p>';

  const recipeNotesHtml = recipeNotes
    ? `
        <div class="note-box">
          <strong>Recipe notes:</strong>
          ${recipeNotes}
        </div>
      `
    : '';

  const overallRatingHtml = recipe.rating === null || recipe.rating === undefined
    ? '<p class="recipe-rating-summary recipe-rating-summary--empty">Not rated yet</p>'
    : `
        <div class="recipe-rating-summary">
          <span class="recipe-rating-summary__label">Overall rating</span>
          <span class="recipe-rating-summary__stars">${starText(recipe.rating)}</span>
          <strong>${Number(recipe.rating).toFixed(1)}</strong>
        </div>
      `;

  dialogContent.innerHTML = `
    <div class="recipe-detail">
      <p class="eyebrow">${recipe.badge}</p>
      <h2>${recipe.title}</h2>

      <p class="subtitle">
        ${recipe.prep} prep • ${recipe.cook} cook • serves ${recipe.serves}
      </p>

      ${overallRatingHtml}

      <div class="recipe-detail__actions">
        <button class="secondary-btn revision-history__toggle" type="button">
          Revision ${recipe.revisionNumber || 1}
        </button>
        <a
          class="secondary-btn recipe-source__link"
          href="${API}/recipes/${encodeURIComponent(recipe.id)}/source"
          target="_blank"
          rel="noopener"
          hidden
        >
          View original recipe
        </a>
        ${kitchenUserIsAuthenticated ? `
        <button class="secondary-btn recipe-grocery__button" type="button">
          Add ingredients to Grocery
        </button>
        ` : ''}
        ${recipe.canEdit ? `
        <button class="primary-btn recipe-edit__button" type="button">
          Edit recipe
        </button>
        <button
          class="recipe-delete__button recipe-delete__icon"
          type="button"
          title="Delete Recipe"
          aria-label="Delete Recipe"
        >
          🗑
        </button>
        ` : ''}
      </div>

      <div class="revision-history" hidden></div>

      <div class="detail-grid">
        <section class="detail-section">
          <h3>Ingredients</h3>
          <ul>
            ${recipe.ingredients.map(item => `<li>${item}</li>`).join('')}
          </ul>
        </section>

        <section class="detail-section">
          <h3>Instructions</h3>
          <ol>
            ${recipe.steps.map(step => `<li>${step}</li>`).join('')}
          </ol>
        </section>
      </div>

      ${recipeNotesHtml}

      <section class="cook-log">
        <div class="cook-log__header">
          <button
            class="secondary-btn cook-log__toggle"
            type="button"
            aria-expanded="false"
          >
            ▶ Cook Log (${cookLog.length})
          </button>

          ${kitchenUserIsAuthenticated ? `
          <button
            class="secondary-btn cook-log__add"
            type="button"
          >
            + Add Entry
          </button>
          ` : ''}
        </div>

        <div class="cook-log__entries cook-log__entries--collapsed">
          ${cookLogEntriesHtml}
        </div>
      </section>
    </div>
  `;

  const toggle = dialogContent.querySelector('.cook-log__toggle');
  const entries = dialogContent.querySelector('.cook-log__entries');
  const addEntry = dialogContent.querySelector('.cook-log__add');
  const editRecipeButton = dialogContent.querySelector('.recipe-edit__button');
  const deleteRecipeButton = dialogContent.querySelector('.recipe-delete__button');
  const revisionToggle = dialogContent.querySelector('.revision-history__toggle');
  const revisionHistory = dialogContent.querySelector('.revision-history');
  const recipeSourceLink = dialogContent.querySelector('.recipe-source__link');
  const groceryButton = dialogContent.querySelector('.recipe-grocery__button');

  if (recipeSourceLink) {
    fetch(`${API}/recipes/${encodeURIComponent(recipe.id)}/source-info`)
      .then(response => response.ok ? response.json() : null)
      .then(source => {
        if (source?.exists) {
          recipeSourceLink.hidden = false;
          recipeSourceLink.textContent =
            source.kind === 'website'
              ? 'View source website'
              : 'View original recipe';
        }
      })
      .catch(error => {
        console.debug('No recipe source available.', error);
      });
  }

  groceryButton?.addEventListener('click', async () => {
    const groceryItems = Array.isArray(recipe.shopping) && recipe.shopping.length
      ? recipe.shopping
      : recipe.ingredients || [];

    if (!groceryItems.length) {
      showGroceryResultDialog(
        'Nothing to send',
        'This recipe does not have any shopping items to send to Grocery.'
      );
      return;
    }

    const originalText = groceryButton.textContent;
    groceryButton.disabled = true;
    groceryButton.textContent = 'Sending to Grocery…';

    try {
      const response = await fetch(
        `${GROCERY_API}/shopping/things-we-need/from-recipe`,
        {
          method: 'POST',
          credentials: 'include',
          headers: {
            'Content-Type': 'application/json'
          },
          body: JSON.stringify({
            recipeId: recipe.id,
            recipeTitle: recipe.title,
            ingredients: groceryItems
          })
        }
      );

      if (response.status === 401) {
        throw new Error(
          'Grocery needs you to sign in first. Open Grocery Excursion, sign in, then try again.'
        );
      }

      let body = null;
      try {
        body = await response.json();
      } catch {}

      if (!response.ok) {
        throw new Error(
          body?.error ||
          body?.message ||
          `Grocery returned HTTP ${response.status}.`
        );
      }

      const added = Number(body?.addedCount || 0);
      const skipped = Number(body?.skippedCount || 0);

      const message =
        `${added} ingredient${added === 1 ? '' : 's'} added to Things We Need.` +
        (skipped
          ? ` ${skipped} ${skipped === 1 ? 'was' : 'were'} already on the list.`
          : '');

      showGroceryResultDialog(
        added > 0 ? 'Sent to Grocery' : 'Already on the list',
        `${recipe.title}: ${message}`
      );
    } catch (error) {
      console.error(error);
      showGroceryResultDialog(
        'Could not send to Grocery',
        error.message || 'The ingredients were not sent to Grocery.'
      );
    } finally {
      groceryButton.disabled = false;
      groceryButton.textContent = originalText;
    }
  });

  editRecipeButton?.addEventListener('click', () => {
    recipeDialog.close();

    // Let the browser finish removing the recipe dialog from the modal/top layer
    // and restore the page scroll lock before opening the editor dialog.
    requestAnimationFrame(() => {
      openRecipeEditor(recipe);
    });
  });

  deleteRecipeButton?.addEventListener('click', async () => {
    const firstWarning = window.confirm(
      `Delete “${recipe.title}”?\n\n` +
      'This will permanently delete the recipe, its revision history, cook logs, ratings, and recipe photos.'
    );

    if (!firstWarning) return;

    const finalWarning = window.confirm(
      `LAST CHANCE\n\n` +
      `Permanently delete “${recipe.title}”?\n\n` +
      'This cannot be undone.'
    );

    if (!finalWarning) return;

    deleteRecipeButton.disabled = true;
    deleteRecipeButton.textContent = '…';

    try {
      const response = await fetch(
        `${API}/recipes/${encodeURIComponent(recipe.id)}`,
        { method: 'DELETE' }
      );

      if (handleUnauthorizedResponse(response)) return;

      if (!response.ok) {
        let message = `HTTP ${response.status}`;
        try {
          const body = await response.json();
          message = body.message || message;
        } catch {}

        throw new Error(message);
      }

      recipes = recipes.filter(item => item.id !== recipe.id);
      buildFilters();
      render();
      recipeDialog.close();
    } catch (error) {
      console.error(error);
      window.alert(
        `The recipe was not deleted.\n\n${error.message || 'Unknown error.'}`
      );

      deleteRecipeButton.disabled = false;
      deleteRecipeButton.textContent = '🗑';
    }
  });

  revisionToggle?.addEventListener('click', async () => {
    if (!revisionHistory) return;

    if (!revisionHistory.hidden) {
      revisionHistory.hidden = true;
      return;
    }

    revisionToggle.disabled = true;
    revisionToggle.textContent = 'Loading history…';

    try {
      const response = await fetch(
        `${API}/recipes/${encodeURIComponent(recipe.id)}/revisions`
      );

      if (!response.ok) throw new Error(`HTTP ${response.status}`);

      const history = await response.json();

      revisionHistory.innerHTML = history.length
        ? history.map(item => {
            const when = new Date(item.createdUtc).toLocaleString();
            const note = item.changeNote
              ? `<div class="revision-history__note">${item.changeNote}</div>`
              : '';

            return `
              <div class="revision-history__item">
                <strong>Revision ${item.revisionNumber}</strong>
                <span>${when} • ${item.createdBy}</span>
                ${note}
              </div>
            `;
          }).join('')
        : '<p>No revision history yet.</p>';

      revisionHistory.hidden = false;
    } catch (error) {
      console.error(error);
      revisionHistory.innerHTML = '<p>Revision history could not be loaded.</p>';
      revisionHistory.hidden = false;
    } finally {
      revisionToggle.disabled = false;
      revisionToggle.textContent = `Revision ${recipe.revisionNumber || 1}`;
    }
  });

  toggle?.addEventListener('click', () => {
    const willExpand =
      toggle.getAttribute('aria-expanded') !== 'true';

    toggle.setAttribute('aria-expanded', String(willExpand));
    toggle.textContent =
      `${willExpand ? '▼' : '▶'} Cook Log (${cookLog.length})`;

    entries.classList.toggle(
      'cook-log__entries--collapsed',
      !willExpand
    );
  });

  addEntry?.addEventListener('click', () => {
    activeCookLogRecipe = recipe;
    cookLogForm.reset();

    cookLogDialog.showModal();
    cookLogNote.focus();
  });

  dialogContent.querySelectorAll('.cook-rating-form').forEach(form => {
    form.addEventListener('submit', async event => {
      event.preventDefault();

      const rater = form.elements.rater.value.trim();
      const stars = Number(form.elements.stars.value);
      const cookLogId = Number(form.dataset.cookLogId);

      if (!rater || !Number.isInteger(stars) || stars < 1 || stars > 5) return;

      const saveButton = form.querySelector('button[type="submit"]');
      saveButton.disabled = true;
      saveButton.textContent = 'Saving…';

      try {
        await saveCookRating(recipe, cookLogId, rater, stars);
        render();
        openRecipe(recipe);

        const reopenedEntries = dialogContent.querySelector('.cook-log__entries');
        const reopenedToggle = dialogContent.querySelector('.cook-log__toggle');
        reopenedEntries?.classList.remove('cook-log__entries--collapsed');
        reopenedToggle?.setAttribute('aria-expanded', 'true');
        if (reopenedToggle) reopenedToggle.textContent = `▼ Cook Log (${cookLog.length})`;
      } catch (error) {
        console.error(error);
        alert('The rating could not be saved.');
        saveButton.disabled = false;
        saveButton.textContent = 'Save rating';
      }
    });
  });

  if (!recipeDialog.open) {
    lockRecipePageScroll();
    recipeDialog.showModal();
  }

  requestAnimationFrame(() => {
    dialogContent.scrollTop = 0;
  });
}
  
function openCooking(recipe) {
  cookingTitle.textContent = recipe.title;
  cookingSteps.innerHTML = recipe.steps.map(step => `<li>${step}</li>`).join('');
  cookingDialog.showModal();
}

function openShopping(recipe) {
  shoppingContent.innerHTML = `
    <div class="recipe-detail">
      <p class="eyebrow">Shopping list</p>
      <h2>${recipe.title}</h2>
      <p class="shopping-note">Tap each box as you shop. Exact H-E-B aisles can be added as we verify them.</p>
      <ul class="shopping-list">
        ${recipe.shopping.map((item, index) => `
          <li><label><input type="checkbox" id="shop-${recipe.id}-${index}"><span>${item}</span></label></li>
        `).join('')}
      </ul>
    </div>`;
  shoppingDialog.showModal();
}

function lockRecipePageScroll() {
  recipePageScrollY = window.scrollY;

  document.body.style.position = 'fixed';
  document.body.style.top = `-${recipePageScrollY}px`;
  document.body.style.left = '0';
  document.body.style.right = '0';
  document.body.style.width = '100%';
}

function restoreRecipePageScroll() {
  const previousScrollBehavior =
    document.documentElement.style.scrollBehavior;

  document.documentElement.style.scrollBehavior = 'auto';

  document.body.style.position = '';
  document.body.style.top = '';
  document.body.style.left = '';
  document.body.style.right = '';
  document.body.style.width = '';

  window.scrollTo(0, recipePageScrollY);

  requestAnimationFrame(() => {
    document.documentElement.style.scrollBehavior =
      previousScrollBehavior;
  });
}

recipeDialog.addEventListener('close', restoreRecipePageScroll);

searchInput.addEventListener('input', render);
document.querySelector('#closeCooking').addEventListener('click', () => cookingDialog.close());
document.querySelector('#installHint').addEventListener('click', () => {
  alert('On your iPhone, open this site in Safari, tap Share, then choose “Add to Home Screen.”');
});

closeRecipeDialog.addEventListener('click', () => {
  recipeDialog.close();
});

accountButton?.addEventListener('click', event => {
  event.stopPropagation();

  if (!kitchenUserIsAuthenticated) {
    window.location.assign(`${API}/auth/login`);
    return;
  }

  if (!accountMenu) return;

  const isOpen = !accountMenu.hidden;
  accountMenu.hidden = isOpen;
  accountButton.setAttribute('aria-expanded', String(!isOpen));
});

document.addEventListener('click', event => {
  if (
    accountMenu &&
    !accountMenu.hidden &&
    !accountMenu.contains(event.target) &&
    !accountButton?.contains(event.target)
  ) {
    accountMenu.hidden = true;
    accountButton?.setAttribute('aria-expanded', 'false');
  }
});

recipeHeroPhoto?.addEventListener('change', updateRecipeHeroPreview);

addRecipeButton?.addEventListener('click', () => openRecipeEditor());
importRecipeButton?.addEventListener('click', openRecipeImport);

recipeImportModes.forEach(button => {
  button.addEventListener('click', () => {
    setRecipeImportMode(button.dataset.importMode);
  });
});

closeRecipeImport?.addEventListener('click', () => recipeImportDialog.close());
cancelRecipeImport?.addEventListener('click', () => recipeImportDialog.close());
recipeImportForm?.addEventListener('submit', importRecipeDraft);


recipeEditorTabs.forEach((tab, index) => {
  tab.addEventListener('click', () => {
    setRecipeEditorTab(tab.dataset.recipeTab);
  });

  tab.addEventListener('keydown', event => {
    if (!['ArrowLeft', 'ArrowRight', 'Home', 'End'].includes(event.key)) return;

    event.preventDefault();

    let nextIndex = index;
    if (event.key === 'ArrowLeft') nextIndex = (index - 1 + recipeEditorTabs.length) % recipeEditorTabs.length;
    if (event.key === 'ArrowRight') nextIndex = (index + 1) % recipeEditorTabs.length;
    if (event.key === 'Home') nextIndex = 0;
    if (event.key === 'End') nextIndex = recipeEditorTabs.length - 1;

    setRecipeEditorTab(recipeEditorTabs[nextIndex].dataset.recipeTab, true);
  });
});


closeRecipeEditor?.addEventListener('click', () => {
  pendingRecipeSource = null;
  recipeEditorDialog.close();
});
cancelRecipeEditor?.addEventListener('click', () => {
  pendingRecipeSource = null;
  recipeEditorDialog.close();
});

recipeTitle?.addEventListener('input', () => {
  if (recipeSlug.dataset.userEdited === 'true') return;
  recipeSlug.value = slugifyRecipeTitle(recipeTitle.value);
});

recipeSlug?.addEventListener('input', () => {
  recipeSlug.dataset.userEdited = 'true';
});

recipeEditorForm?.addEventListener('submit', saveRecipe);

cookLogForm.addEventListener('submit', async event => {
  event.preventDefault();

  const note = cookLogNote.value.trim();

  if (!note || !activeCookLogRecipe) return;

  try {
    const response = await fetch(
      `${API}/recipes/${activeCookLogRecipe.id}/cook-log`,
      {
        method: 'POST',
        headers: {
          'Content-Type': 'application/json'
        },
        body: JSON.stringify({
          author: 'Randy',
          note
        })
      }
    );

    if (handleUnauthorizedResponse(response)) return;

    if (!response.ok) {
      throw new Error(`HTTP ${response.status}`);
    }

    const savedEntry = await response.json();

    activeCookLogRecipe.journal ??= {};
    activeCookLogRecipe.journal.cookLog ??= [];
    activeCookLogRecipe.journal.cookLog.unshift(savedEntry);

    cookLogDialog.close();
    openRecipe(activeCookLogRecipe);
  } catch (error) {
    console.error(error);
    alert('The cook log entry could not be saved.');
  }
});

  

Promise.all([
  loadAuthenticatedUser(),
  loadRecipes()
]).then(() => updateFilterToggleLabel());


closeGroceryResult?.addEventListener('click', closeGroceryResultDialog);
groceryResultOk?.addEventListener('click', closeGroceryResultDialog);
groceryResultDialog?.addEventListener('click', event => {
  if (event.target === groceryResultDialog) {
    closeGroceryResultDialog();
  }
});
