import './Story.css'
import './css/asideStory.css'
import './css/mainStory.css'

import { useState, useRef} from 'react'
import { useNavigate } from 'react-router-dom'

import useStory from './useStory'

import PlayerIcon from '../../assets/icons/player.png'
import Clock from '../../assets/icons/clock.png'
import Home from '../../assets/buttons/home.png'

const Players = [
    {id: 1, icon: PlayerIcon ,name: 'Pedro'},
    {id: 2, icon: PlayerIcon ,name: 'Lucas'},
    {id: 3, icon: PlayerIcon ,name: 'Rafael'}, 
    {id: 4, icon: PlayerIcon ,name: 'Davi'},
    {id: 5, icon: PlayerIcon ,name: 'Leandra'},
    {id: 6, icon: PlayerIcon ,name: 'Giih'},   
]

function Story() {

  const { currentPlayer, storyParts, seconds, addMessage } = useStory(Players, 30)
  const [text, setText] = useState("")
  const messagesEndRef = useRef(null)
  const textRef = useRef(null)
  const navigate = useNavigate()

  const handleSubmit = () => {
    addMessage(text)
    setText("")
    textRef.current?.focus()
  }


  return (

    <section className='story'>

      <aside className='aside-story'>

        <ul className="players-container">
          {Players.map((player) => (
            <li key={player.id} className='player-card'>
              <img className='player-icon' src={player.icon} alt={player.name}/>
              <span className="player-name">{player.name}</span>
            </li>
          ))}
        </ul>

        <div className="timer-container">
          <img className='ClockIcon' src={Clock} alt="Relógio" />
          <div className="infs-time">
            <span className="player-timer">Turno: {currentPlayer.name}</span>
            <span className='timer'>{seconds}s</span>
          </div>
        </div>

      </aside>

    <main className='main-story'>

        <section className="stories">

          <header className="story-header">
            <h2>Como Treinar o Seu Dragão</h2>
            <button onClick={() => navigate('/home')} aria-label='Voltar para a home'>
              <img className='icon-home' src={Home} alt=''/>
            </button>

          </header>

          <div className="parts-of-story">

            <div className="parts" ref={messagesEndRef}>
              {storyParts.map((part, i) => {
                const player = Players[part.playerIndex]
                return (
                  <div key={i} className="player-message">
                    <img src={player.icon} alt={player.name} className='player-message-icon'/>
                    <span className="current-player-name">{player.name}</span>
                    <p>{part.text}</p>
                  </div>
                )
              })}
            </div>
            
          </div>

        </section>

        <div className="interaction-container">

          <textarea
            ref={textRef}
            value={text}
            onChange={(e) => setText(e.target.value)}
            placeholder='Era uma vez...'
            name="contribution"
            id="contribution"
            aria-label='Contribuição para a história'
          />
          <button onClick={handleSubmit} type='button'>
            Enviar
          </button>
          
        </div>

    </main>

    </section>

  )

}

export default Story;