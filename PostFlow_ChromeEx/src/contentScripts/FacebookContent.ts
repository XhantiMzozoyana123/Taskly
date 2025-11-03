import { Lead } from '../domain/Leads';
import { SearchDto } from '../dtos/SearchDto';

console.log("FacebookContent.ts loaded.");

// Function to simulate scraping and send data to background script
async function scrapeFacebookPosts(searchDto: SearchDto) {
  console.log(`Facebook content script: Starting scrape for query "${searchDto.query}"`);
  const scrapedLeads: Lead[] = [];

  // Simulate scraping posts from the DOM
  // In a real scenario, this would involve complex DOM traversal and observation
  const postElements = document.querySelectorAll('div[role="article"], ._5pcb, ._4-u8'); // Example selectors

  postElements.forEach((element, index) => {
    if (index < 5) { // Limit to first 5 for demonstration
      const nameElement = element.querySelector('strong a, .fwb a');
      const profileUrlElement = nameElement ? (nameElement as HTMLAnchorElement) : null;
      const postDescriptionElement = element.querySelector('.userContent, ._5pbx');
      const postLinkElement = element.querySelector('a._5pcq'); // Link to the post itself

      const name = nameElement?.textContent?.trim() || `Facebook User ${index}`;
      const profileUrl = profileUrlElement?.href || `https://facebook.com/profile/${index}`;
      const postDescription = postDescriptionElement?.textContent?.trim() || `No description for post ${index}`;
      const postUrl = postLinkElement?.href || window.location.href;

      const lead: Lead = {
        name,
        profileUrl,
        postDescription,
        postUrl,
        platform: 'Facebook',
        keywords: searchDto.keywords.join(','),
        query: searchDto.query,
        status: 'New',
        postDate: new Date(),
      };
      scrapedLeads.push(lead);
    }
  });

  console.log(`Facebook content script: Scraped ${scrapedLeads.length} leads.`);
  // Send scraped leads to the background script for processing (relevance check, saving to backend)
  chrome.runtime.sendMessage({
    action: 'scrapedLeads',
    data: scrapedLeads
  });
}

// Listen for messages from the background script
chrome.runtime.onMessage.addListener((message, sender, sendResponse) => {
  if (message.action === 'performScraping') {
    const searchDto: SearchDto = {
      platform: message.data.platform,
      query: message.data.query,
      keywords: message.data.keywords
    };
    scrapeFacebookPosts(searchDto);
    sendResponse({ success: true, message: 'Scraping started in content script' });
  }
  return true; // Keep the message channel open for sendResponse
});
