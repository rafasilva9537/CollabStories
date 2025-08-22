import './Gallery.css'

import { useState } from 'react'

import GalleryImg from '../../../../assets/imgs/gallery.png'
import Collaborators from '../../../../assets/imgs/collaborators.png'
import Like from '../../../../assets/icons/like.png'

const GalleryCart = [
    {
        img: GalleryImg,
        title: 'Como treinar o seu Dragão',
        description: 'Treinar seu dragão envolve construir confiança, ensinar comandos básicos e praticar juntos regularmente. Comece com pequenas tarefas, recompense comportamentos positivos e seja paciente. Gradualmente, aumente a dificuldade e diversifique os desafios, sempre respeitando os limites do dragão. A chave é parceria e consistência, não força bruta.',
        likes: 11,
        collaborators: 3,
    },
    {
        img: GalleryImg,
        title: 'Branca de Neve e os Sete Anões',
        description: 'Branca de Neve é uma princesa cuja madrasta tenta eliminá-la por inveja de sua beleza. Ela encontra refúgio na casa dos sete anões, aprende sobre amizade, coragem e gentileza, enfrentando desafios com inteligência e coração puro. Uma história clássica sobre bondade, inveja e superação.',
        likes: 20,
        collaborators: 4,
    }

]

function Gallery() {

const [selectedStory, setSelectedStory] = useState(null);

    return (

        <section className="gallery">

            <ul className="carrossel-gallery">

                    {GalleryCart.map((item, i) => (
                        <li key={i} className="card-story">    
                            <img src={GalleryImg} alt="Imagem de Galeria" />
                            <div className="story-content-card">
                                <h4>
                                    {item.title}
                                </h4>
                                <div className="gallery-story-stats">
                                    <div className="stats-item">
                                        <img src={Like} alt={`Quantidade de Likes ${item.likes}`} />
                                        <span>
                                            {item.likes}
                                        </span>
                                    </div>
                                    <div className="stats-item">
                                        <img src={Collaborators} alt={`Quantidade de colaboradores ${item.collaborators}`} />
                                        <span>
                                            {item.collaborators}
                                        </span>
                                    </div>
                                </div>
                            </div>
                        </li>
                    ))}

            </ul>
            <h3 className='story-in-carrossel'>
                Galeria
            </h3>

        </section>

    )
}

export default Gallery;