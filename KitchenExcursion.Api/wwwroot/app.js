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
  { key: 'status', label: 'Status', values: ['favorite', 'la-jefa-approved', 'quick'] }
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
  quick: 'Quick'
};

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

        return `
          <article class="cook-log__entry">
            <div class="cook-log__meta">
              <strong>${formattedDate} • ${formattedTime}</strong>
              <div class="cook-log__author">
                by ${entry.author}
              </div>
            </div>

            <p>${entry.note.replace(/\s+/g, ' ').trim()}</p>
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

  dialogContent.innerHTML = `
    <div class="recipe-detail">
      <p class="eyebrow">${recipe.badge}</p>
      <h2>${recipe.title}</h2>

      <p class="subtitle">
        ${recipe.prep} prep • ${recipe.cook} cook • serves ${recipe.serves}
      </p>

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

          <button
            class="secondary-btn cook-log__add"
            type="button"
          >
            + Add Entry
          </button>
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

  lockRecipePageScroll();
  recipeDialog.showModal();

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

  

loadRecipes();
updateFilterToggleLabel();
