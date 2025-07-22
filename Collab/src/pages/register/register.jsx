import { useNavigate } from 'react-router-dom';

import './Register.css'
import './RegisterStyles/RegisterAside.css'
import './RegisterStyles/RegisterMain.css'

import ImgRegister from '../../assets/register.png'
import arroba from '../../assets/arroba.png'

function Register(){

const navigate = useNavigate();

return(

    <>
    
        <section className='Register'>

            <aside className='AsideRegister'>
                <img className='ImgRegister'
                src={ImgRegister} alt="Imagem de Cadastro" />
                <span onClick={() => navigate('/login')}
                className='link'>
                    Já possui cadastro?
                </span>
            </aside>
            <main className='MainRegister'>

                <div className="H1Container">

                    <h1>
                        CADASTRAR
                    </h1>

                </div>
                <section className="Register-MainContainer">

                <form action="" className='Register-Form'>
                    <div className="container-Register-name-user">
                        <input type="text" placeholder='Nome'/>
                        <input type="text" placeholder='Sobrenome' />
                    </div>
                    <div className="container-Register-IdPlayer">
                        <img className='arroba' 
                        src={arroba} alt="Crie um identificador de usuário" />
                        <input className='Input-Register-Id'
                        type="text" placeholder='Crie um @ para seu usuário'/>
                    </div>
                    <div className='container-Register-data'>
                        <input className='Input-Register-EmAndPassw'
                        type="email" placeholder='email@gmail.com'/>
                        <input className='Input-Register-EmAndPassw'
                        type="password" placeholder='Crie uma senha'/>
                    </div>
                    <button className='CreateAccount-Button'>
                        Criar Conta
                    </button>
                </form>

                </section>

            </main>

        </section>
    
    </>

)

}

export default Register;