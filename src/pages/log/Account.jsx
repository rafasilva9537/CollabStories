import { useState } from "react";
import { useRegister, useLogin } from "./useAccount";
import "./Account.css";

import LoginIcon from "../../assets/imgs/login.png";
import Email from "../../assets/icons/email.png";
import Padlock from "../../assets/icons/padlock.png";

function Account() {

  const [ActiveForm, setActiveForm] = useState("register");

  return (

    <section className="account">

      <article className="article-account">

        <aside className="aside-account">

          <img src={LoginIcon} alt="Imagem de login" />

        </aside>
        <main className="main-account">

          <nav>

            <div className="btns-container">
              <button
                className={`btn-account ${ActiveForm === "register" ? "active" : ""}`}
                id="register-btn"
                onClick={() => setActiveForm("register")}
              >
                Registrar-se
              </button>
              <button
                className={`btn-account ${ActiveForm === "login" ? "active" : ""}`}
                id="login-btn"
                onClick={() => setActiveForm("login")}
              >
                Acessar conta
              </button>
            </div>
            
          </nav>
          {ActiveForm === "register" && <Register />}
          {ActiveForm === "login" && <Login />}

        </main>

      </article>

    </section>

  );

}

function Register() {

  const { formData, error, success, handleInputChange, handleSubmit } = useRegister();

  return (

    <form className="register" onSubmit={handleSubmit}>

      <h2>Registrar-se</h2>

      {error && <div className="error-message">{error}</div>}
      {success && <div className="success-message">{success}</div>}

      <div className="name-container">
        <input
          type="text"
          className="username-log"
          name="displayName"
          placeholder="Nome exibido"
          value={formData.displayName}
          onChange={handleInputChange}
        />
        <input
          type="text"
          className="username-log"
          name="username"
          placeholder="Id de usuário"
          value={formData.username}
          onChange={handleInputChange}
        />
      </div>
      <div className="email-input">
        <img className="imgs-log" src={Email} alt="Imagem do email" />
        <input
          type="email"
          className="inputs-log"
          name="email"
          placeholder="Email"
          value={formData.email}
          onChange={handleInputChange}
        />
      </div>
      <div className="password-input">
        <img className="imgs-log" src={Padlock} alt="Imagem da senha" />
        <input
          type="password"
          className="inputs-log"
          name="password"
          placeholder="Crie uma senha"
          value={formData.password}
          onChange={handleInputChange}
        />
      </div>
      <div className="password-input">
        <img className="imgs-log" src={Padlock} alt="Imagem da senha" />
        <input
          type="password"
          className="inputs-log"
          name="confirmPassword"
          placeholder="Repita a senha"
          value={formData.confirmPassword}
          onChange={handleInputChange}
        />
      </div>
      <button type="submit" className="button-log">
        Criar Conta!
      </button>

    </form>

  );

}

function Login() {
  const { formData, error, handleInputChange, handleSubmit } = useLogin();

  return (

    <form className="login" onSubmit={handleSubmit}>

      <h2>Acessar Conta</h2>

      {error && <div className="error-message">{error}</div>}

      <div className="email-input">

        <img className="imgs-log" src={Email} alt="Imagem do email" />
        <input
          type="email"
          className="inputs-log"
          name="email"
          placeholder="Email"
          value={formData.email}
          onChange={handleInputChange}
        />

      </div>

      <div className="password-input">
        <img className="imgs-log" src={Padlock} alt="Imagem da senha" />
        <input
          type="password"
          className="inputs-log"
          name="password"
          placeholder="Senha"
          value={formData.password}
          onChange={handleInputChange}
        />

      </div>
      <button type="submit" className="button-log">
        Entrar!
      </button>

    </form>
  );
  
}

export default Account;