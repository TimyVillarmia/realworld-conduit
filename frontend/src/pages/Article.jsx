import React, { useEffect } from 'react'
import { useParams } from 'react-router-dom'
import { ArticleMeta, ArticleComments } from '../components'
import { useArticleQuery } from '../hooks'
import useViewArticleMutation from '../hooks/useViewArticleMutation'

function Article() {
  const { slug } = useParams()
  const { data } = useArticleQuery()
  const { mutate: viewArticle } = useViewArticleMutation(slug)

  // Trigger increment on mount
  useEffect(() => {
    if (slug) {
      viewArticle()
    }
  }, [slug, viewArticle])

  const { title, description, body, viewCount } = data.article

  return (
    <div className="article-page">
      <div className="banner">
        <div className="container">
          <h1>{title}</h1>
          {/* RENDER THE EMOJI HERE */}
          <span className="view-count" style={{ marginLeft: '1.5rem', marginRight: '1rem' }}>
            👁️ {viewCount}
          </span>
          <ArticleMeta />
        </div>
      </div>
      <div className="container page">
        <div className="row article-content">
          <div className="col-md-12">
            <p>{description}</p>
            <p>{body}</p>
          </div>
        </div>
        <hr />
        <div className="article-actions">
          <ArticleMeta />
        </div>
        <div className="row">
          <div className="col-xs-12 col-md-8 offset-md-2">
            <ArticleComments />
          </div>
        </div>
      </div>
    </div>
  )
}

export default Article
