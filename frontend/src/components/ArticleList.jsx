import React from 'react'
import { isNil } from 'lodash-es'
import useDeepCompareEffect from 'use-deep-compare-effect'
import { useArticlesQuery } from '../hooks'
import ArticlePreview from './ArticlePreview'

const initialFilters = { author: null, favorited: null, tag: null, offset: null, feed: false }
const limit = 10

function ArticleList({ filters = initialFilters }) {
  const [offset, setOffset] = React.useState(0)
  const { data, isFetching, isError, isSuccess } = useArticlesQuery({ filters: { ...filters, offset } })

  useDeepCompareEffect(() => {
    if (!isNil(filters.offset)) {
      setOffset(filters.offset)
    }
  }, [filters])

  // 1. Loading and Error states
  if (isFetching) return <p className="article-preview">Loading articles...</p>
  if (isError) return <p className="article-preview">Loading articles failed :(</p>

  // 2. Defensive Data Extraction
  const articles = data?.articles || []
  const articlesCount = data?.articlesCount || 0
  const pages = Math.ceil(articlesCount / limit)

  // 3. Empty State
  if (isSuccess && articles.length === 0) {
    return <p className="article-preview">No articles are here... yet.</p>
  }

  return (
    <>
      {articles.map((article) => (
        <ArticlePreview key={article.slug} article={article} />
      ))}
      {pages > 1 && (
        <nav>
          <ul className="pagination">
            {Array.from({ length: pages }, (_, i) => (
              <li className={offset === i ? 'page-item active' : 'page-item'} key={i}>
                <button type="button" className="page-link" onClick={() => setOffset(i)}>
                  {i + 1}
                </button>
              </li>
            ))}
          </ul>
        </nav>
      )}
    </>
  )
}

export default ArticleList
