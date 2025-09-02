import './Story.css'
import './css/asideStory.css'
import './css/mainStory.css'

import { useState, useRef} from 'react'
import { useNavigate } from 'react-router-dom'

import useStory from './useStory'

import PlayerIcon from '../../assets/icons/player.png'
import Home from '../../assets/icons/home.png'
import AddPlayer from '../../assets/icons/add_player.png'

const Players = [
    {id: 'dsousr', icon: PlayerIcon ,name: 'Pedro'},  
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
                  <div className="player-infs-aside">
                    <span className="player-name">{player.name}</span>
                    <span>@{player.id}</span>
                  </div>
                </li>
            ))}

            {Array.from({ length: 5 }).map((_, i) => (
                <li key={`add-${i}`} className="Add-player-container">
                    <button>
                        <img src={AddPlayer} alt="Adicionar jogador" />
                    </button>
                    <div className="player-add">
                        <span>
                            Adicionar Jogador
                        </span>
                        <input type="text" placeholder='@...'/>
                    </div>
                </li>
            ))}
        </ul>

    </aside>

    <main className='main-story'>

        <section className="stories">

          <header className="story-header">

            <div className="header-infs">
                <h2>Como Treinar o Seu Dragão</h2>
                <span className='gender'>
                    Aventura
                </span>
            </div>
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
                    <span className="current-player-id">{player.id}</span>
                    <p>{part.text}</p>
                  </div>
                )
              })}
            </div>
            
          </div>

        </section>

        <div className="main-itens">

        <div className="timer-container">

          <span className="current-player">
            Vez de: {currentPlayer?.name}
          </span>
          <span className="timer">
            {seconds}s
          </span>

        </div>

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
        </div>

    </main>

    </section>

  )

}

export default Story;