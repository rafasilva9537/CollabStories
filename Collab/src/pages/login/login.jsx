import { useNavigate } from 'react-router-dom';

import './login.css'
import './LoginStyles/LoginAside.css'
import './LoginStyles/LoginMain.css'

import login from '../../assets/login.png'

function Login(){

const navigate = useNavigate();

return(

    <>
    
    <section className="Login">

        <aside className='AsideLogin'>

            <img className='ImgLogin'
            src={login} alt="Login" />
            <span onClick={() => navigate('/register')}
            className="link">
                Não possui cadastro?
            </span>

        </aside>
        <main className='MainLogin'>

            <div className="H1Container">

                <h1>
                    Acessar Conta!
                </h1>

            </div>
            <section className='Login-Data'>

                <form action="" className="form-login">

                    <input placeholder='Email cadastrado'
                    type="email" className="input-login" />
                    <input placeholder='Digite sua senha'
                    type="password" className="input-login" />
                    <button className='button-login'>
                        Entrar
                    </button>

                </form>

            </section>

        </main>

    </section>

    </>

)

}

export default Login;