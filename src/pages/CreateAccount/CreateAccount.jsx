import './CreateAccount.css'
import Sign from '../../assets/sign.png'
import { useNavigate } from 'react-router-dom';

function CreateAccount(){

const navigate = useNavigate();

return(

    <div className="container">
    
            <main>
    
                <aside>
                    <img src={Sign} alt='Logo'/>
                </aside>
                <section>
    
                    <h1>
                        Cadastrar
                    </h1>
                    <form action="">

                        <div className="inputsName">

                            <input className='inputname' type="name" placeholder='Nome' />
                            <input className='inputname' type="name" placeholder='Sobrenome' />

                        </div>
    
                        <input className='inputLogin' type="email" placeholder='Digite seu email'/>
                        <input  className='inputLogin' type="password" placeholder='Crie uma senha' />
    
                        <button>
                            Cadastrar
                        </button>
    
                    </form>
                    <div className="sign">
                        <span onClick={() => navigate('/Login')}>
                            Já possui cadastro?
                        </span>
                    </div>
    
                </section>
    
            </main>
    
        </div>

)
 
}

export default CreateAccount;