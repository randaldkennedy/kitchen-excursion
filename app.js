const recipes = [
  {
    id: 'smothered-pork-chops',
    title: "Randy's Smothered Pork Chops",
    category: ['pork', 'favorite'],
    badge: 'Approved',
    image: 'assets/smothered-pork-chops.jpeg',
    imageAlt: 'Smothered pork chop with mushroom gravy, rice, and broccoli',
    summary: 'Bone-in chops with mushrooms, onions, and gravy. Next time: one-hour bake, creamier gravy, mashed potatoes.',
    prep: '20 min',
    cook: '1 hr',
    serves: '2–4',
    ingredients: [
      '4 bone-in center loin pork chops, about 1 inch thick',
      '1 large yellow onion, sliced into ¼-inch half-moons',
      '16 oz sliced mushrooms',
      '1 can cream of mushroom soup',
      '1 packet onion soup mix',
      '1 cup beef broth',
      '½ cup whole milk or half-and-half',
      '1 tablespoon Worcestershire sauce',
      'Salt and black pepper',
      'Butter or oil for browning',
      'Mashed potatoes for serving'
    ],
    steps: [
      'Preheat the oven to 350°F.',
      'Season the pork chops with salt and pepper.',
      'Brown the chops in a skillet with a little butter or oil for 2–3 minutes per side. They do not need to cook through.',
      'Remove the chops. Cook the onions for 2–3 minutes, then add the mushrooms and cook another 4–5 minutes.',
      'Stir in the cream of mushroom soup, onion soup mix, beef broth, milk, and Worcestershire sauce. Scrape up the browned bits from the skillet.',
      'Place the chops in a baking dish, pour the gravy over them, and cover tightly with foil.',
      'Bake for 1 hour. Check for tenderness and doneness; add only 5–10 minutes if truly needed.',
      'Rest for 5 minutes and serve over mashed potatoes with plenty of gravy.'
    ],
    notes: 'First attempt at 1 hour 15 minutes was slightly dry and tough near the bone. The outer meat was perfect. Keep the generous amount of mushrooms and onions; there was plenty of gravy.'
  },
  {
    id: 'mississippi-pot-roast',
    title: 'Mississippi Pot Roast',
    category: ['beef', 'slow-cooker'],
    badge: 'Next up',
    image: '',
    imageAlt: '',
    summary: 'Five ingredients, almost no prep, and built for mashed potatoes.',
    prep: '10 min',
    cook: '8 hr low',
    serves: '6–8',
    ingredients: [
      '3–4 lb chuck roast',
      '1 packet Hidden Valley Ranch seasoning',
      '1 packet au jus gravy mix',
      '1 stick butter',
      '6–10 pepperoncini peppers',
      '2–4 tablespoons pepperoncini juice (optional)',
      'Mashed potatoes for serving'
    ],
    steps: [
      'Place the chuck roast in the slow cooker.',
      'Sprinkle the ranch seasoning and au jus mix evenly over the roast.',
      'Place the stick of butter on top.',
      'Add the pepperoncini peppers around the roast and a little juice if desired.',
      'Cover and cook on LOW for about 8 hours, or HIGH for about 5 hours.',
      'When fork-tender, shred with two forks and stir the meat into the cooking juices.',
      'Serve over mashed potatoes.'
    ],
    notes: 'Do not add water. Start checking a little early if the roast is closer to 3 lb. Save all the cooking juices for the potatoes.'
  }
];

const grid = document.querySelector('#recipeGrid');
const template = document.querySelector('#recipeCardTemplate');
const searchInput = document.querySelector('#searchInput');
const recipeDialog = document.querySelector('#recipeDialog');
const dialogContent = document.querySelector('#dialogContent');
const cookingDialog = document.querySelector('#cookingDialog');
const cookingTitle = document.querySelector('#cookingTitle');
const cookingSteps = document.querySelector('#cookingSteps');
let activeFilter = 'all';

function render() {
  const query = searchInput.value.trim().toLowerCase();
  const filtered = recipes.filter(recipe => {
    const matchesFilter = activeFilter === 'all' || recipe.category.includes(activeFilter);
    const haystack = [recipe.title, recipe.summary, recipe.ingredients.join(' '), recipe.notes].join(' ').toLowerCase();
    return matchesFilter && haystack.includes(query);
  });
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
    node.querySelector('.view-btn').addEventListener('click', () => openRecipe(recipe));
    node.querySelector('.cook-btn').addEventListener('click', () => openCooking(recipe));
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

document.querySelectorAll('.filter').forEach(button => {
  button.addEventListener('click', () => {
    document.querySelectorAll('.filter').forEach(btn => btn.classList.remove('active'));
    button.classList.add('active');
    activeFilter = button.dataset.filter;
    render();
  });
});
searchInput.addEventListener('input', render);
document.querySelector('#closeCooking').addEventListener('click', () => cookingDialog.close());
document.querySelector('#installHint').addEventListener('click', () => alert('After we publish it, open the site in Safari on your iPhone, tap Share, then choose “Add to Home Screen.”'));
render();
