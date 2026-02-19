import React from 'react'
import ReactDOM from 'react-dom'
import { QueryClient, QueryClientProvider } from 'react-query'
import { ReactQueryDevtools } from 'react-query/devtools'
import axios from 'axios'
import App from './App'

// --- API CONFIGURATION ---
// Replace this URL with your actual ASP.NET local URL (e.g., https://localhost:7001)
const LOCAL_BACKEND_URL = 'http://localhost:5000'
// @ts-ignore
if (process.env.NODE_ENV === 'production') {
  axios.defaults.baseURL = 'https://api.realworld.io/api'
} else {
  axios.defaults.baseURL = LOCAL_BACKEND_URL
}

axios.interceptors.request.use((config) => {
  if (config.url && config.url.startsWith('/')) {
    // Remove leading slash, capitalize first letter, put slash back
    const path = config.url.substring(1)
    // eslint-disable-next-line no-param-reassign
    config.url = `/${path.charAt(0).toUpperCase()}${path.slice(1)}`
  }
  return config
})

// Default fetcher for React Query
const defaultQueryFn = async ({ queryKey }) => {
  const [url, params] = queryKey
  const { data } = await axios.get(url, { params })
  return data
}

const queryClient = new QueryClient({
  defaultOptions: {
    queries: {
      queryFn: defaultQueryFn,
      staleTime: 300000,
    },
  },
})

// --- MOCK SERVER LOGIC ---
// We keep Cypress for testing, but disabled the dev mock server
// @ts-ignore
if (window.Cypress && process.env.NODE_ENV === 'test') {
  // eslint-disable-next-line global-require
  const { createServer } = require('miragejs')
  const cyServer = createServer({
    routes() {
      ;['get', 'put', 'patch', 'post', 'delete'].forEach((method) => {
        // @ts-ignore
        this[method]('/*', (schema, request) => window.handleFromCypress(request))
      })
    },
  })
  cyServer.logging = false
}
/* REMOVED: MirageJS dev server. 
  By removing this, requests will now actually hit your ASP.NET API.
*/

// @ts-ignore
ReactDOM.render(
  <React.StrictMode>
    <QueryClientProvider client={queryClient}>
      <App />
      <ReactQueryDevtools initialIsOpen={false} containerElement="div" />
    </QueryClientProvider>
  </React.StrictMode>,
  document.getElementById('root'),
)
