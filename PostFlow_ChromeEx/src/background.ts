import { FacebookService } from './services/FacebookService';
import { InstagramService } from './services/InstagramService';
import { SearchDto } from './dtos/SearchDto';
import { Lead } from './domain/Leads';

const facebookService = new FacebookService();
const instagramService = new InstagramService();

chrome.runtime.onMessage.addListener((message, sender, sendResponse) => {
  if (message.action === 'startScraping') {
    const searchDto: SearchDto = message.data;
    console.log('Background script received startScraping:', searchDto);

    // Determine which service to use based on platform
    let service: FacebookService | InstagramService;
    if (searchDto.platform === 'Facebook') {
      service = facebookService;
    } else if (searchDto.platform === 'Instagram') {
      service = instagramService;
    } else {
      console.error('Unknown platform:', searchDto.platform);
      sendResponse({ success: false, error: 'Unknown platform' });
      return true;
    }

    // Execute scraping in the content script
    chrome.tabs.query({ active: true, currentWindow: true }, (tabs) => {
      if (tabs[0] && tabs[0].id) {
        chrome.scripting.executeScript({
          target: { tabId: tabs[0].id },
          function: (platform: string, query: string, keywords: string[]) => {
            // This function runs in the context of the content script
            // It should trigger the actual scraping logic in FacebookContent.ts or InstagramContent.ts
            // and send results back to the background script.
            console.log(`Content script instructed to scrape ${platform} for query "${query}"`);
            chrome.runtime.sendMessage({
              action: 'performScraping',
              data: { platform, query, keywords }
            });
          },
          args: [searchDto.platform, searchDto.query, searchDto.keywords]
        });
      }
    });

    sendResponse({ success: true, message: 'Scraping initiated' });
    return true; // Keep the message channel open for sendResponse
  } else if (message.action === 'stopScraping') {
    console.log('Background script received stopScraping');
    // TODO: Implement logic to stop ongoing scraping if any
    sendResponse({ success: true, message: 'Scraping stopped' });
    return true;
  } else if (message.action === 'scrapedLeads') {
    const leads: Lead[] = message.data;
    console.log('Background script received scraped leads:', leads);
    // Here, the background script could further process leads or send them to the backend
    // For now, the service already handles sending to backend, so we just log.
    // We might want to update the popup UI with the new lead count.
    chrome.runtime.sendMessage({ action: 'updateLeadCount', data: leads.length });
    sendResponse({ success: true, message: 'Leads processed' });
    return true;
  }
});

// Initial setup for the extension (e.g., context menus, alarms)
chrome.runtime.onInstalled.addListener(() => {
  console.log('PostFlow Extension installed.');
});
