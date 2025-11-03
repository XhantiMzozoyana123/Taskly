export const API_BASE_URL = 'http://localhost:5000/api'; // Assuming a local ASP.NET Core API running on port 5000

export const API_ENDPOINTS = {
  LEADS: `${API_BASE_URL}/leads`,
  AI_CHECK_RELEVANCE: `${API_BASE_URL}/ai/check-relevance`,
  AI_GENERATE_DM: `${API_BASE_URL}/ai/generate-dm`,
};
