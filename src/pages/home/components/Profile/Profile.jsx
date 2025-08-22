import './Profile.css'
import { useState } from 'react'

import PlayerIcon from '../../../../assets/icons/player.png'
import Gallery from '../../../../assets/imgs/gallery.png'
import Like from '../../../../assets/icons/like.png'
import Collaborators from '../../../../assets/imgs/collaborators.png'
import Search from '../../../../assets/icons/search.png'

function Profile(){

  const [searchPlayer, setSearchPlayer] = useState(false)
  const [showSearch, setShowSearch] = useState(false)

  const Stats = [
      {iconStat: Collaborators, title: 'Seguidores', count: '5'},
      {iconStat: Gallery, title: 'Histórias', count: '2'},
      {iconStat: Like, title: 'Curtidas', count: '24'}
  ]

  const OpenSearchPlayer = () => {
      setSearchPlayer(true)
      setTimeout(()=>{
          setShowSearch(true)
      },0)
  }

  const OpenMyProfile = () => {
      setSearchPlayer(false)
      setTimeout(()=>{
          setShowSearch(false)
      }, 300)
  }

  if(showSearch){
      return <SearchPlayer Stats={Stats} OpenMyProfile={OpenMyProfile}/>
  }

  return(

    <article className={`profile-card ${searchPlayer ? 'slide-out' : ''}`}>
    
        <button className="button-change"
        onClick={OpenSearchPlayer}>

            <img src={PlayerIcon} alt="Perfil" />

        </button>

        <div className="player-infs-container">

            <h2>Pedro</h2>
            <span className="profile-player-id">@dsousr</span>
            <p>
              Meu gênero favorito de histórias é terror! Adoro criar histórias
              que colocam medo em qualquer um!
            </p>
            <div className="profile-player-stats">
                {Stats.map((stat, i) => (
                <div key={i} className="profile-container-stat">
                    <img src={stat.iconStat} alt={stat.title} />
                    <span>{stat.count}</span>
                </div>
                ))}
            </div>

        </div>
    
    </article>

  )

}

function SearchPlayer({ Stats, OpenMyProfile }){

  return(

    <article className='search-player'>

        <div className="player-infs-container">

            <div className="search-players-container">
                <input placeholder='Qual o id @' type="text" />
                <button className='button-search'>
                    Pesquisar
                </button>
            </div>

        </div>

        <button className="button-change"
        onClick={OpenMyProfile}>

            <img src={Search} alt="Pesquisar jogador" />

        </button>

    </article>

  )
  
}

export default Profile;