import { useState, useEffect } from 'react'

export default function useStory(Players, initialSeconds = 30) {
  const [currentPlayerIndex, setCurrentPlayerIndex] = useState(0)
  const [seconds, setSeconds] = useState(initialSeconds)
  const [storyParts, setStoryParts] = useState([])

  useEffect(() => {
    const timer = setInterval(() => {
      setSeconds(prev => {
        if (prev <= 1) {
          setCurrentPlayerIndex(idx => (idx + 1) % Players.length)
          return initialSeconds
        }
        return prev - 1
      })
    }, 1000)

    return () => clearInterval(timer)
  }, [Players.length, initialSeconds])

  const currentPlayer = Players[currentPlayerIndex]

  const addMessage = (text) => {
    if (!text.trim()) return
    setStoryParts(prev => [...prev, { text, playerIndex: currentPlayerIndex }])
    setCurrentPlayerIndex((currentPlayerIndex + 1) % Players.length)
    setSeconds(initialSeconds)
  }

  return { currentPlayer, storyParts, seconds, addMessage }
}