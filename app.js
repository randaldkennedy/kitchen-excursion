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

let recipes = [];
let activeFilter = 'all';

const filterLabels = {
  all: 'All',
  favorite: 'Favorites',
  pork: 'Pork',
  beef: 'Beef',
  'slow-cooker': 'Slow cooker'
};

async function loadRecipes() {
  try {
    const response = await fetch('data/recipes.json', { cache: 'no-store' });
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
  const found = new Set(['all']);
  recipes.forEach(recipe => recipe.filters.forEach(filter => found.add(filter)));
  filtersContainer.innerHTML = '';
  [...found].forEach(filter => {
    const button = document.createElement('button');
    button.type = 'button';
    button.className = `filter${filter === activeFilter ? ' active' : ''}`;
    button.dataset.filter = filter;
    button.textContent = filterLabels[filter] || filter;
    button.addEventListener('click', () => {
      activeFilter = filter;
      document.querySelectorAll('.filter').forEach(btn => btn.classList.toggle('active', btn === button));
      render();
    });
    filtersContainer.appendChild(button);
  });
}

function filteredRecipes() {
  const query = searchInput.value.trim().toLowerCase();
  return recipes.filter(recipe => {
    const matchesFilter = activeFilter === 'all' || recipe.filters.includes(activeFilter);
    const haystack = [
      recipe.title,
      recipe.summary,
      recipe.categories.join(' '),
      recipe.ingredients.join(' '),
      recipe.notes
    ].join(' ').toLowerCase();
    return matchesFilter && haystack.includes(query);
  });
}

function render() {
  const filtered = filteredRecipes();
  recipeCount.textContent = `${filtered.length} recipe${filtered.length === 1 ? '' : 's'}`;
  grid.innerHTML = '';
  if (!filtered.length) {
    grid.innerHTML = '<p class="empty">No recipes match that search yet.</p>';
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
  dialogContent.innerHTML = `
    <div class="recipe-detail">
      <p class="eyebrow">${recipe.badge}</p>
      <h2>${recipe.title}</h2>
      <p class="subtitle">${recipe.prep} prep • ${recipe.cook} cook • serves ${recipe.serves}</p>
      <div class="detail-grid">
        <section class="detail-section">
          <h3>Ingredients</h3>
          <ul>${recipe.ingredients.map(item => `<li>${item}</li>`).join('')}</ul>
        </section>
        <section class="detail-section">
          <h3>Instructions</h3>
          <ol>${recipe.steps.map(step => `<li>${step}</li>`).join('')}</ol>
        </section>
      </div>
      <div class="note-box"><strong>Randy's notes:</strong> ${recipe.notes}</div>
    </div>`;
  recipeDialog.showModal();
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

searchInput.addEventListener('input', render);
document.querySelector('#closeCooking').addEventListener('click', () => cookingDialog.close());
document.querySelector('#installHint').addEventListener('click', () => {
  alert('On your iPhone, open this site in Safari, tap Share, then choose “Add to Home Screen.”');
});

loadRecipes();
