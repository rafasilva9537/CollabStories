import './home.css'

import NewStory from '../../assets/buttons/newStory.png'
import Player from '../../assets/buttons/player.png'
import Gallery from '../../assets/buttons/gallery.png'

function Home(){

return(

    <>
    <div className="Home">
        <header className='Header-Home'>

            <h1 className='Title-Home'>
                Página Inicial
            </h1>

        </header>
        <section className='section-home'>

            <div className="Buttons-Home">

                <div className="Button-Home">
                    <img className='Icons-Home'
                    src={Player} alt="Meu perfil" />
                </div>
                <div className="Button-Home">
                    <img className='Icons-Home'
                    src={Gallery} alt="Galeria" />
                </div>
                <div className="Button-Home">
                    <img className='Icons-Home'
                    src={NewStory} alt="Criar história" />
                </div>

            </div>
            <main className='main-content-home'>

            </main>

        </section>
    </div>
    </>

)

}

export default Home;