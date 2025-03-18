document.querySelector('.enviar-hist').addEventListener('click', function() {
    const inputHist = document.querySelector('.input-hist');
    const inputText = inputHist.value.trim();

    if (inputText !== '') {
        const balao = document.createElement('div');
        balao.classList.add('balao-p');
        balao.textContent = inputText;

        const historiasContadas = document.querySelector('.historias-contadas');
        historiasContadas.appendChild(balao);

        historiasContadas.scrollTop = historiasContadas.scrollHeight;

        inputHist.value = '';
    }
});
