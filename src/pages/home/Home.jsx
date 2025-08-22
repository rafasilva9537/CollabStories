import './Home.css'

import Profile from './components/Profile/Profile'
import Gallery from './components/Gallery/Gallery';
import CreateStory from './components/CreateStory/CreateStory';

function Home(){

return(

    <section className='home'>

        <main className="main-home">

            <Profile/>
            <CreateStory/>

        </main>
        <section className="gallery-content">

            <Gallery/>

        </section>

    </section>

)

}

export default Home;