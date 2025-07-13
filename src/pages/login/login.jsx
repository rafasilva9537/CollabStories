import './login.css'
import Logo from '../../assets/logo.png'
import { useNavigate } from 'react-router-dom';

function Login(){

const navigate = useNavigate();

return(

    <div className="container">

        <main>

            <aside>
                <img src={Logo} alt='Logo'/>
            </aside>
            <section>

                <h1>
                    Acessar Conta
                </h1>
                <form action="handlerSubmit">

                    <input className='inputLogin' type="email" placeholder='Digite seu email'/>
                    <input  className='inputLogin' type="password" placeholder='Digite sua senha' />

                    <button>
                        Login
                    </button>

                </form>
                <div className="sign">
                    <span onClick={() => navigate('/')}>
                        não possui cadastro?
                    </span>
                </div>

            </section>

        </main>

    </div>

)

}

export default Login;