const usuarios = [
    { username: "Henrique", email: "henrique@gmail.com", senha: "123456" },
    { username: "Rafael", email: "rafael@gmail.com", senha: "senha123" },
    { username: "Davi", email: "davi@gmail.com", senha: "abcde" }
];

function verificarLogin() {

    const usuarioInput = document.querySelector(".inputs-login[type='text']").value;
    const senhaInput = document.querySelector(".inputs-login[type='text']:nth-of-type(2)").value;

    const usuarioEncontrado = usuarios.find(user => 
        (user.username === usuarioInput || user.email === usuarioInput) && user.senha === senhaInput
    );

    if (usuarioEncontrado) {
        alert("Login bem-sucedido!");
    } else {
        alert("Usuário ou senha incorretos");
    }
}

document.querySelector(".LoginButton").addEventListener("click", verificarLogin);

//Cadastro de email:

document.querySelector(".VerificaDisp").addEventListener("click", () => {
    const emailInput = document.querySelector(".input-create").value;
    const mensagem = document.querySelector(".mensagem-verificacao");
    const formActions = document.querySelector(".ActionsCreate");

    if (usuarios.some(user => user.email === emailInput)) {
        mensagem.textContent = "Email já cadastrado!";
        mensagem.style.color = "red";
    } else {

        mensagem.textContent = "Email disponível!";

        const inputEmail = document.querySelector(".input-create");
        inputEmail.remove();

        const inputSenha = document.createElement("input");
        inputSenha.classList.add("input-senha");
        inputSenha.type = "password";
        inputSenha.placeholder = "Crie uma senha";

        const botaoCriarConta = document.createElement("button");
        botaoCriarConta.classList.add("CriarContaButton");
        botaoCriarConta.textContent = "Criar Conta";


        const botaoVerificar = document.querySelector(".VerificaDisp");
        botaoVerificar.replaceWith(botaoCriarConta);


        formActions.appendChild(inputSenha);

        botaoCriarConta.addEventListener("click", () => {
            const senhaInput = inputSenha.value;

            if (senhaInput) {
                usuarios.push({ username: emailInput.split('@')[0], email: emailInput, senha: senhaInput });

                mensagem.textContent = "Conta criada com sucesso!";
                mensagem.style.color = "green";
                inputSenha.remove();
                botaoCriarConta.remove();
            } else {
                mensagem.textContent = "Insira uma senha.";
                mensagem.style.color = "red";
            }
        });
    }
});
