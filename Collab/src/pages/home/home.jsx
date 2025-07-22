import { useState } from 'react';
import './home.css';

import NewStoryIcon from '../../assets/buttons/newStory.png';
import ProfileIcon from '../../assets/buttons/player.png';
import GalleryIcon from '../../assets/buttons/gallery.png';

import GalleryPage from './Mains/mainGallery';
import ProfilePage from './Mains/mainProfile';
import NewStoryPage from './Mains/mainNewStory';

function Home() {
  const [currentComponent, setCurrentComponent] = useState(<ProfilePage />);

  return (
    <div className="Home">
      <header className="Header-Home">
        <h1 className="Title-Home">Página Inicial</h1>
      </header>

      <section className="section-home">
        <div className="Buttons-Home">
          <div className="Button-Home" onClick={() => setCurrentComponent(<ProfilePage />)}>
            <img className="Icons-Home" src={ProfileIcon} alt="Profile" />
          </div>
          <div className="Button-Home" onClick={() => setCurrentComponent(<GalleryPage />)}>
            <img className="Icons-Home" src={GalleryIcon} alt="Gallery" />
          </div>
          <div className="Button-Home" onClick={() => setCurrentComponent(<NewStoryPage />)}>
            <img className="Icons-Home" src={NewStoryIcon} alt="Create Story" />
          </div>
        </div>

        <main className="main-content-home">
          {currentComponent}
        </main>
      </section>
    </div>
  );
}

export default Home;
