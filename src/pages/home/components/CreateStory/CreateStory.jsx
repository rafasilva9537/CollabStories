import './CreateStory.css'
import { useNavigate } from 'react-router-dom';

import StoryIcon from '../../../../assets/imgs/story-icon.png'

function CreateStory() {
  const navigate = useNavigate();

  return (

    <section className='to-create-story'>

      <div className="story-icon-container">

        <img src={StoryIcon} alt="Seção de criação de história" />

      </div>
      <div className="story-settings">

        <div className="story-title-container">

          <input placeholder='Coloque o título aqui!' id='title' type="text" />
          <label id='title'>Criar história</label>

        </div>
          <select id="genre" name="genre" defaultValue="">
            <option value="" disabled>Selecione um gênero</option>
            <option value="action">Ação</option>
            <option value="adventure">Aventura</option>
            <option value="comedy">Comédia</option>
            <option value="sci-fi">Ficção científica</option>
            <option value="fantasy">Fantasia</option>
            <option value="romance">Romance</option>
            <option value="mystery">Mistério</option>
          </select>
          <textarea placeholder='Qual a descrição?' id="description"/>
          
          <button onClick={() => navigate('/')}>
            Começar!
          </button>

      </div>

    </section>

  )

}

export default CreateStory;