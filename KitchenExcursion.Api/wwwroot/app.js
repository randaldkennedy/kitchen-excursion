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
const recipeEditorDialog = document.querySelector('#recipeEditorDialog');
const recipeEditorForm = document.querySelector('#recipeEditorForm');
const recipeTitle = document.querySelector('#recipeTitle');
const recipeSlug = document.querySelector('#recipeSlug');
const recipeEditorStatus = document.querySelector('#recipeEditorStatus');
const closeRecipeEditor = document.querySelector('#closeRecipeEditor');
const cancelRecipeEditor = document.querySelector('#cancelRecipeEditor');
const saveRecipeButton = document.querySelector('#saveRecipeButton');
const accountButton = document.querySelector('#accountButton');
const accountMenu = document.querySelector('#accountMenu');
const accountName = document.querySelector('#accountName');
const accountInitial = document.querySelector('#accountInitial');
const accountMenuName = document.querySelector('#accountMenuName');
const accountEmail = document.querySelector('#accountEmail');
const recipeEditorTabs = [...document.querySelectorAll('[data-recipe-tab]')];
const recipeEditorPanels = [...document.querySelectorAll('[data-recipe-panel]')];

const API = '/api';

let activeCookLogRecipe = null;
let recipePageScrollY = 0;


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

function openRecipeEditor() {
  if (!requireKitchenSignIn()) return;

  recipeEditorForm.reset();
  recipeEditorForm.elements.rater.value = 'Randy';
  recipeEditorStatus.textContent = '';
  recipeSlug.dataset.userEdited = 'false';
  setRecipeEditorTab('details');
  recipeEditorDialog.showModal();
  recipeTitle.focus();
}

async function saveNewRecipe(event) {
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
    image: String(form.get('image') || '') || null,
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
    shopping: linesToArray(String(form.get('shopping') || ''))
  };

  saveRecipeButton.disabled = true;
  saveRecipeButton.textContent = 'Saving…';
  recipeEditorStatus.textContent = '';

  try {
    const createResponse = await fetch(`${API}/recipes`, {
      method: 'POST',
      headers: { 'Content-Type': 'application/json' },
      body: JSON.stringify(payload)
    });

    if (handleUnauthorizedResponse(createResponse)) return;

    if (!createResponse.ok) {
      let message = `HTTP ${createResponse.status}`;
      try {
        const body = await createResponse.json();
        message = body.message || message;
      } catch {}
      throw new Error(message);
    }

    const created = await createResponse.json();
    const cookLogNote = String(form.get('cookLogNote') || '').trim();
    const rater = String(form.get('rater') || '').trim();
    const starsValue = String(form.get('stars') || '').trim();

    if (cookLogNote) {
      const ratings = starsValue && rater
        ? [{ rater, stars: Number(starsValue) }]
        : [];

      const cookResponse = await fetch(
        `${API}/recipes/${encodeURIComponent(created.id)}/cook-log`,
        {
          method: 'POST',
          headers: { 'Content-Type': 'application/json' },
          body: JSON.stringify({
            author: rater || 'Randy',
            note: cookLogNote,
            ratings
          })
        }
      );

      if (handleUnauthorizedResponse(cookResponse)) return;

      if (!cookResponse.ok) {
        throw new Error(`Recipe saved, but the first cook log failed (HTTP ${cookResponse.status}).`);
      }
    }

    const refreshedResponse = await fetch(`${API}/recipes/${encodeURIComponent(created.id)}`);
    if (!refreshedResponse.ok) throw new Error(`Recipe saved, but reload failed (HTTP ${refreshedResponse.status}).`);
    const refreshed = await refreshedResponse.json();

    recipes.push(refreshed);
    buildFilters();
    render();

    recipeEditorDialog.close();
    openRecipe(refreshed);
  } catch (error) {
    console.error(error);
    recipeEditorStatus.textContent = error.message || 'The recipe could not be saved.';
  } finally {
    saveRecipeButton.disabled = false;
    saveRecipeButton.textContent = 'Save Recipe';
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

addRecipeButton?.addEventListener('click', openRecipeEditor);


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


closeRecipeEditor?.addEventListener('click', () => recipeEditorDialog.close());
cancelRecipeEditor?.addEventListener('click', () => recipeEditorDialog.close());

recipeTitle?.addEventListener('input', () => {
  if (recipeSlug.dataset.userEdited === 'true') return;
  recipeSlug.value = slugifyRecipeTitle(recipeTitle.value);
});

recipeSlug?.addEventListener('input', () => {
  recipeSlug.dataset.userEdited = 'true';
});

recipeEditorForm?.addEventListener('submit', saveNewRecipe);

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
