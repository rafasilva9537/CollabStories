import './story.css';
import PlayerIcon from '../../assets/icons/user.png';

function Story() {
  const players = [
    { id: 1, name: 'Pedro Henrique', icon: PlayerIcon },
    { id: 2, name: 'Rafael Silva', icon: PlayerIcon },
    { id: 3, name: 'Nanda Beatriz', icon: PlayerIcon },
    { id: 4, name: 'Davi Silva', icon: PlayerIcon }
  ];

  return (

    <>
        <header className="header-story">
        {players.map((player) => (
            <div key={player.id} className="player-card">
            <img
                src={player.icon}
                alt={`Ícone de ${player.name}`}
                className="icon-player"
            />
            <span className="player-name">
                {player.name}
            </span>
            </div>
        ))}
        </header>

        <section className='titleStory'>
            <h2 className="title">
                Como treinar o seu Dragão
            </h2>
        </section> 
        <section className="containerBlockStory">

            <section className="blockStory">

                <article className="box">



                </article>


                <form className='formSend'>
                    <input type="text" className='InputSend' placeholder='Coloque aqui sua parte'/>
                    <button className='Send'>
                        Enviar
                    </button>
                </form>

            </section>
            <aside className='infsTime'>

            </aside>

        </section>
    </> 

  );
}

export default Story;
