document.addEventListener('DOMContentLoaded', () => {
  const startScrapingButton = document.getElementById('startScraping') as HTMLButtonElement;
  const stopScrapingButton = document.getElementById('stopScraping') as HTMLButtonElement;
  const platformSelect = document.getElementById('platform') as HTMLSelectElement;
  const queryInput = document.getElementById('query') as HTMLInputElement;
  const keywordsInput = document.getElementById('keywords') as HTMLInputElement;
  const leadCountSpan = document.getElementById('leadCount') as HTMLSpanElement;

  // Placeholder for lead count from local storage or backend
  let leadCount = 0;
  leadCountSpan.textContent = leadCount.toString();

  startScrapingButton.addEventListener('click', () => {
    const platform = platformSelect.value;
    const query = queryInput.value;
    const keywords = keywordsInput.value.split(',').map(k => k.trim()).filter(k => k.length > 0);

    console.log(`Starting scraping for ${platform} with query "${query}" and keywords "${keywords.join(', ')}"`);
    // TODO: Send message to background script to start scraping
  });

  stopScrapingButton.addEventListener('click', () => {
    console.log('Stopping scraping.');
    // TODO: Send message to background script to stop scraping
  });

  // TODO: Implement logic to update leadCountSpan from background script or local storage
});
