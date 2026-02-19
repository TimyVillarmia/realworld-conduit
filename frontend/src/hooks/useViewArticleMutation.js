import axios from 'axios'
import { useMutation, useQueryClient } from 'react-query'

function useViewArticleMutation(slug) {
  const queryClient = useQueryClient()
  const queryKey = `/articles/${slug}`

  return useMutation(() => axios.post(`/articles/${slug}/viewed`), {
    onSuccess: ({ data }) => {
      // Since your Tier 1 returns the WHOLE article,
      // we update the cache immediately so the UI reflects the new count.
      queryClient.setQueryData(queryKey, data)
    },
  })
}

export default useViewArticleMutation
