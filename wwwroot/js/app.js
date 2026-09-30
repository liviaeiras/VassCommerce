"use strict";

console.log("VassCommerce: app.js carregado");

const $ = id => document.getElementById(id);

const elementos = {
    listaCategorias: $("listaCategorias"),
    listaProdutos: $("listaProdutos"),
    resultadoProdutos: $("resultadoProdutos"),
    ordenacaoProdutos: $("ordenacaoProdutos"),
    formPesquisa: $("formPesquisa"),
    campoPesquisa: $("campoPesquisa"),

    botaoCarrinho: $("botaoCarrinho"),
    carrinho: $("carrinho"),
    fecharCarrinho: $("fecharCarrinho"),
    overlay: $("overlay"),
    quantidadeCarrinho: $("quantidadeCarrinho"),
    itensCarrinho: $("itensCarrinho"),
    valorTotalCarrinho: $("valorTotalCarrinho"),
    botaoFinalizar: $("botaoFinalizar"),

    botaoConta: $("botaoConta"),
    textoConta: $("textoConta"),

    modalAutenticacao: $("modalAutenticacao"),
    fecharAutenticacao: $("fecharAutenticacao"),
    abaLogin: $("abaLogin"),
    abaCadastro: $("abaCadastro"),
    formLogin: $("formLogin"),
    formCadastro: $("formCadastro"),
    mensagemAutenticacao: $("mensagemAutenticacao"),

    botaoDadosCliente: $("botaoDadosCliente"),
    botaoEndereco: $("botaoEndereco"),
    botaoPagamento: $("botaoPagamento"),
    botaoPedidos: $("botaoPedidos"),
    resultadoCliente: $("resultadoCliente")
};

let produtosAtuais = [];
let produtosCarrinho = lerLocalStorage(
    "vasscommerce_carrinho",
    []
);
let usuarioAtual = lerLocalStorage(
    "vasscommerce_usuario",
    null
);

// =====================================================
// LOCAL STORAGE
// =====================================================

function lerLocalStorage(chave, valorPadrao) {
    try {
        const valor = localStorage.getItem(chave);

        return valor
            ? JSON.parse(valor)
            : valorPadrao;
    } catch {
        localStorage.removeItem(chave);
        return valorPadrao;
    }
}

function salvarCarrinho() {
    localStorage.setItem(
        "vasscommerce_carrinho",
        JSON.stringify(produtosCarrinho)
    );
}

function obterToken() {
    return localStorage.getItem(
        "vasscommerce_token"
    );
}

function encerrarSessao() {
    localStorage.removeItem(
        "vasscommerce_token"
    );

    localStorage.removeItem(
        "vasscommerce_usuario"
    );

    usuarioAtual = null;

    atualizarEstadoConta();
}

// =====================================================
// FUNÇÕES AUXILIARES
// =====================================================

function formatarPreco(valor) {
    return Number(valor || 0).toLocaleString(
        "pt-BR",
        {
            style: "currency",
            currency: "BRL"
        }
    );
}

function formatarData(valor) {
    if (!valor) {
        return "";
    }

    const data = new Date(valor);

    if (Number.isNaN(data.getTime())) {
        return "";
    }

    return data.toLocaleDateString("pt-BR");
}

function escaparHtml(texto) {
    const elemento = document.createElement("div");

    elemento.textContent = texto ?? "";

    return elemento.innerHTML;
}

function normalizarTexto(texto) {
    return String(texto ?? "")
        .normalize("NFD")
        .replace(/[\u0300-\u036f]/g, "")
        .toLowerCase();
}

function limparCpf(cpf) {
    return String(cpf ?? "")
        .replace(/\D/g, "");
}

function atualizarEstadoConta() {
    if (usuarioAtual?.nomeCompleto) {
        elementos.textoConta.textContent =
            usuarioAtual.nomeCompleto.split(" ")[0];

        return;
    }

    elementos.textoConta.textContent =
        "Minha conta";
}

function mostrarResultadoCliente(html) {
    elementos.resultadoCliente.innerHTML = html;
}

function mostrarCarregamentoCliente(mensagem) {
    mostrarResultadoCliente(`
        <p class="carregando">
            ${escaparHtml(mensagem)}
        </p>
    `);
}

function mostrarErroCliente(mensagem) {
    mostrarResultadoCliente(`
        <p class="erro">
            ${escaparHtml(mensagem)}
        </p>
    `);
}

// =====================================================
// API
// =====================================================

async function lerMensagemErro(
    resposta,
    mensagemPadrao
) {
    try {
        const dados = await resposta.json();

        if (dados.mensagem) {
            return dados.mensagem;
        }

        if (dados.errors) {
            const mensagens = Object
                .values(dados.errors)
                .flat()
                .filter(Boolean);

            if (mensagens.length > 0) {
                return mensagens.join(" ");
            }
        }

        return (
            dados.detail ||
            dados.title ||
            mensagemPadrao
        );
    } catch {
        return mensagemPadrao;
    }
}

async function chamarApi(url, opcoes = {}) {
    const token = obterToken();

    const headers = {
        Accept: "application/json",
        ...(opcoes.headers || {})
    };

    if (token) {
        headers.Authorization =
            `Bearer ${token}`;
    }

    let resposta;

    try {
        resposta = await fetch(url, {
            ...opcoes,
            headers
        });
    } catch {
        throw new Error(
            "Não foi possível conectar à API."
        );
    }

    if (resposta.status === 401) {
        encerrarSessao();

        abrirAutenticacao(
            "login",
            "Sua sessão expirou. Entre novamente."
        );

        throw new Error(
            "Sua sessão expirou."
        );
    }

    return resposta;
}

// =====================================================
// AUTENTICAÇÃO
// =====================================================

function abrirAutenticacao(
    modo = "login",
    mensagem = ""
) {
    elementos.modalAutenticacao.classList.add(
        "aberto"
    );

    elementos.modalAutenticacao.setAttribute(
        "aria-hidden",
        "false"
    );

    selecionarAbaAutenticacao(modo);

    elementos.mensagemAutenticacao.textContent =
        mensagem;
}

function fecharModalAutenticacao() {
    elementos.modalAutenticacao.classList.remove(
        "aberto"
    );

    elementos.modalAutenticacao.setAttribute(
        "aria-hidden",
        "true"
    );
}

function selecionarAbaAutenticacao(modo) {
    const cadastroAtivo =
        modo === "cadastro";

    elementos.abaCadastro.classList.toggle(
        "ativa",
        cadastroAtivo
    );

    elementos.abaLogin.classList.toggle(
        "ativa",
        !cadastroAtivo
    );

    elementos.formCadastro.classList.toggle(
        "oculto",
        !cadastroAtivo
    );

    elementos.formLogin.classList.toggle(
        "oculto",
        cadastroAtivo
    );

    elementos.mensagemAutenticacao.textContent =
        "";
}

function exigirLogin() {
    if (obterToken()) {
        return true;
    }

    abrirAutenticacao(
        "login",
        "Entre na sua conta para acessar essa área."
    );

    return false;
}

async function autenticar(url, dados) {
    let resposta;

    try {
        resposta = await fetch(url, {
            method: "POST",

            headers: {
                "Content-Type": "application/json",
                Accept: "application/json"
            },

            body: JSON.stringify(dados)
        });
    } catch {
        throw new Error(
            "Não foi possível conectar à API."
        );
    }

    if (!resposta.ok) {
        throw new Error(
            await lerMensagemErro(
                resposta,
                "Não foi possível autenticar."
            )
        );
    }

    const resultado = await resposta.json();

    localStorage.setItem(
        "vasscommerce_token",
        resultado.token
    );

    usuarioAtual = {
        nomeCompleto: resultado.nomeCompleto,
        clienteId: resultado.clienteId
    };

    localStorage.setItem(
        "vasscommerce_usuario",
        JSON.stringify(usuarioAtual)
    );

    atualizarEstadoConta();

    elementos.formLogin.reset();
    elementos.formCadastro.reset();

    fecharModalAutenticacao();
}

elementos.formLogin.addEventListener(
    "submit",
    async evento => {
        evento.preventDefault();

        elementos.mensagemAutenticacao.textContent =
            "Entrando...";

        try {
            await autenticar(
                "/auth/login",
                {
                    email: $("loginEmail")
                        .value
                        .trim(),

                    senha: $("loginSenha").value
                }
            );
        } catch (erro) {
            elementos.mensagemAutenticacao.textContent =
                erro.message;
        }
    }
);

elementos.formCadastro.addEventListener(
    "submit",
    async evento => {
        evento.preventDefault();

        elementos.mensagemAutenticacao.textContent =
            "Criando sua conta...";

        try {
            await autenticar(
                "/auth/registro",
                {
                    nomeCompleto:
                        $("cadastroNome")
                            .value
                            .trim(),

                    email:
                        $("cadastroEmail")
                            .value
                            .trim(),

                    senha:
                        $("cadastroSenha").value,

                    cpf:
                        limparCpf(
                            $("cadastroCpf").value
                        ),

                    dataNascimento:
                        $("cadastroNascimento").value
                }
            );
        } catch (erro) {
            elementos.mensagemAutenticacao.textContent =
                erro.message;
        }
    }
);

// =====================================================
// CATEGORIAS
// =====================================================

function obterIconeCategoria(nome) {
    const texto = normalizarTexto(nome);

    if (texto.includes("informatica")) {
        return "💻";
    }

    if (texto.includes("livro")) {
        return "📚";
    }

    if (texto.includes("jogo")) {
        return "🎮";
    }

    if (texto.includes("brinquedo")) {
        return "🧸";
    }

    if (texto.includes("papelaria")) {
        return "✏️";
    }

    return "🛍️";
}

async function carregarCategorias() {
    elementos.listaCategorias.innerHTML = `
        <p class="carregando">
            Carregando categorias...
        </p>
    `;

    try {
        const resposta = await chamarApi(
            "/categoria"
        );

        if (!resposta.ok) {
            throw new Error(
                await lerMensagemErro(
                    resposta,
                    "Não foi possível carregar as categorias."
                )
            );
        }

        const categorias = await resposta.json();

        exibirCategorias(categorias);
    } catch (erro) {
        elementos.listaCategorias.innerHTML = `
            <p class="erro">
                ${escaparHtml(erro.message)}
            </p>
        `;
    }
}

function exibirCategorias(categorias) {
    elementos.listaCategorias.innerHTML = "";

    if (!categorias?.length) {
        elementos.listaCategorias.innerHTML = `
            <p class="carregando">
                Nenhuma categoria encontrada.
            </p>
        `;

        return;
    }

    categorias.forEach(categoria => {
        const card =
            document.createElement("article");

        card.className = "categoria-card";
        card.tabIndex = 0;

        card.innerHTML = `
            <div class="categoria-icone">
                ${obterIconeCategoria(
                    categoria.nome
                )}
            </div>

            <h3>
                ${escaparHtml(categoria.nome)}
            </h3>

            <p>
                ${escaparHtml(
                    categoria.descricao
                )}
            </p>
        `;

        const selecionar = () => {
            document
                .querySelectorAll(
                    ".categoria-card"
                )
                .forEach(item => {
                    item.classList.remove(
                        "selecionada"
                    );
                });

            card.classList.add("selecionada");

            carregarProdutos(
                categoria.id,
                categoria.nome
            );

            $("produtos").scrollIntoView({
                behavior: "smooth"
            });
        };

        card.addEventListener(
            "click",
            selecionar
        );

        card.addEventListener(
            "keydown",
            evento => {
                if (
                    evento.key === "Enter" ||
                    evento.key === " "
                ) {
                    evento.preventDefault();
                    selecionar();
                }
            }
        );

        elementos.listaCategorias.appendChild(
            card
        );
    });
}

// =====================================================
// PRODUTOS
// =====================================================

function obterIconeProduto(nome) {
    const texto = normalizarTexto(nome);

    if (texto.includes("notebook")) {
        return "💻";
    }

    if (texto.includes("mouse")) {
        return "🖱️";
    }

    if (texto.includes("teclado")) {
        return "⌨️";
    }

    if (texto.includes("monitor")) {
        return "🖥️";
    }

    if (
        texto.includes("livro") ||
        texto.includes("hobbit") ||
        texto.includes("clean code")
    ) {
        return "📚";
    }

    return "📦";
}

function obterPrecoFinal(produto) {
    const valorPromocional =
        produto.valorComPromocao;

    if (
        valorPromocional !== null &&
        valorPromocional !== undefined &&
        Number(valorPromocional) <
            Number(produto.valorUnitario)
    ) {
        return Number(valorPromocional);
    }

    return Number(produto.valorUnitario);
}

async function carregarProdutos(
    categoriaId,
    categoriaNome
) {
    elementos.listaProdutos.innerHTML = `
        <p class="carregando">
            Carregando produtos...
        </p>
    `;

    try {
        const resposta = await chamarApi(
            `/categoria/${encodeURIComponent(
                categoriaId
            )}/produto`
        );

        if (!resposta.ok) {
            throw new Error(
                await lerMensagemErro(
                    resposta,
                    "Não foi possível carregar os produtos."
                )
            );
        }

        produtosAtuais = await resposta.json();

        exibirProdutos(produtosAtuais);

        elementos.resultadoProdutos.textContent =
            `${produtosAtuais.length} produto(s) em ${categoriaNome}`;
    } catch (erro) {
        produtosAtuais = [];

        elementos.listaProdutos.innerHTML = `
            <p class="erro">
                ${escaparHtml(erro.message)}
            </p>
        `;
    }
}

function exibirProdutos(produtos) {
    elementos.listaProdutos.innerHTML = "";

    if (!produtos?.length) {
        elementos.listaProdutos.innerHTML = `
            <p class="carregando">
                Nenhum produto encontrado.
            </p>
        `;

        return;
    }

    produtos.forEach(produto => {
        const card =
            document.createElement("article");

        card.className = "produto-card";

        const possuiPromocao =
            produto.valorComPromocao !== null &&
            produto.valorComPromocao !== undefined &&
            Number(produto.valorComPromocao) <
                Number(produto.valorUnitario);

        const promocaoHtml = possuiPromocao
            ? `
                <span class="selo-promocao">
                    PROMOÇÃO
                </span>

                <span class="preco-antigo">
                    ${formatarPreco(
                        produto.valorUnitario
                    )}
                </span>
            `
            : "";

        card.innerHTML = `
            <div class="produto-imagem">
                ${obterIconeProduto(produto.nome)}
            </div>

            <div class="produto-conteudo">
                <h3>
                    ${escaparHtml(produto.nome)}
                </h3>

                ${promocaoHtml}

                <strong class="produto-preco">
                    ${formatarPreco(
                        obterPrecoFinal(produto)
                    )}
                </strong>

                <button
                    type="button"
                    class="botao botao-comprar"
                >
                    Adicionar ao carrinho
                </button>
            </div>
        `;

        card
            .querySelector(".botao-comprar")
            .addEventListener(
                "click",
                () => adicionarAoCarrinho(
                    produto
                )
            );

        elementos.listaProdutos.appendChild(card);
    });
}

// =====================================================
// PESQUISA E ORDENAÇÃO
// =====================================================

elementos.ordenacaoProdutos.addEventListener(
    "change",
    () => {
        const produtos = [...produtosAtuais];
        const tipo =
            elementos.ordenacaoProdutos.value;

        if (tipo === "menor-preco") {
            produtos.sort(
                (a, b) =>
                    obterPrecoFinal(a) -
                    obterPrecoFinal(b)
            );
        }

        if (tipo === "maior-preco") {
            produtos.sort(
                (a, b) =>
                    obterPrecoFinal(b) -
                    obterPrecoFinal(a)
            );
        }

        if (tipo === "nome") {
            produtos.sort(
                (a, b) =>
                    a.nome.localeCompare(
                        b.nome,
                        "pt-BR"
                    )
            );
        }

        exibirProdutos(produtos);
    }
);

elementos.formPesquisa.addEventListener(
    "submit",
    evento => {
        evento.preventDefault();

        if (!produtosAtuais.length) {
            elementos.resultadoProdutos.textContent =
                "Primeiro selecione uma categoria.";

            return;
        }

        const termo = normalizarTexto(
            elementos.campoPesquisa.value.trim()
        );

        const encontrados = termo
            ? produtosAtuais.filter(produto =>
                normalizarTexto(
                    produto.nome
                ).includes(termo)
            )
            : produtosAtuais;

        exibirProdutos(encontrados);

        elementos.resultadoProdutos.textContent =
            `${encontrados.length} resultado(s) encontrado(s)`;
    }
);

// =====================================================
// CARRINHO
// =====================================================

function adicionarAoCarrinho(produto) {
    produtosCarrinho.push(produto);

    salvarCarrinho();
    atualizarCarrinho();
    abrirCarrinho();
}

function atualizarCarrinho() {
    elementos.quantidadeCarrinho.textContent =
        produtosCarrinho.length;

    if (!produtosCarrinho.length) {
        elementos.itensCarrinho.innerHTML = `
            <p>Seu carrinho está vazio.</p>
        `;

        elementos.valorTotalCarrinho.textContent =
            formatarPreco(0);

        salvarCarrinho();

        return;
    }

    elementos.itensCarrinho.innerHTML = "";

    let total = 0;

    produtosCarrinho.forEach(
        (produto, indice) => {
            const preco =
                obterPrecoFinal(produto);

            total += preco;

            const item =
                document.createElement("div");

            item.className = "item-carrinho";

            item.innerHTML = `
                <div>
                    <strong>
                        ${escaparHtml(produto.nome)}
                    </strong>

                    <p>
                        ${formatarPreco(preco)}
                    </p>
                </div>

                <button
                    type="button"
                    class="remover-item"
                    aria-label="Remover produto"
                >
                    ✕
                </button>
            `;

            item
                .querySelector(".remover-item")
                .addEventListener(
                    "click",
                    () => {
                        produtosCarrinho.splice(
                            indice,
                            1
                        );

                        salvarCarrinho();
                        atualizarCarrinho();
                    }
                );

            elementos.itensCarrinho.appendChild(
                item
            );
        }
    );

    elementos.valorTotalCarrinho.textContent =
        formatarPreco(total);

    salvarCarrinho();
}

function abrirCarrinho() {
    elementos.carrinho.classList.add("aberto");
    elementos.overlay.classList.add("ativo");

    elementos.overlay.setAttribute(
        "aria-hidden",
        "false"
    );
}

function fecharCarrinhoPainel() {
    elementos.carrinho.classList.remove("aberto");
    elementos.overlay.classList.remove("ativo");

    elementos.overlay.setAttribute(
        "aria-hidden",
        "true"
    );
}

// =====================================================
// CRIAÇÃO DO PEDIDO
// =====================================================

elementos.botaoFinalizar.addEventListener(
    "click",
    async () => {
        if (!produtosCarrinho.length) {
            alert("Seu carrinho está vazio.");
            return;
        }

        if (!exigirLogin()) {
            fecharCarrinhoPainel();
            return;
        }

        elementos.botaoFinalizar.disabled = true;
        elementos.botaoFinalizar.textContent =
            "Criando pedido...";

        try {
            const itens = {};

            produtosCarrinho.forEach(produto => {
                if (!itens[produto.id]) {
                    itens[produto.id] = {
                        produtoId: produto.id,
                        quantidade: 0
                    };
                }

                itens[produto.id].quantidade += 1;
            });

            const resposta = await chamarApi(
                "/cliente/me/pedido",
                {
                    method: "POST",

                    headers: {
                        "Content-Type":
                            "application/json"
                    },

                    body: JSON.stringify({
                        itens: Object.values(itens)
                    })
                }
            );

            if (!resposta.ok) {
                throw new Error(
                    await lerMensagemErro(
                        resposta,
                        "Não foi possível criar o pedido."
                    )
                );
            }

            const pedido = await resposta.json();

            produtosCarrinho = [];

            salvarCarrinho();
            atualizarCarrinho();
            fecharCarrinhoPainel();

            alert(
                `Pedido ${pedido.id} criado. ` +
                `Total: ${formatarPreco(
                    pedido.valorTotal
                )}`
            );

            $("cliente").scrollIntoView({
                behavior: "smooth"
            });

            consultarPedidos();
        } catch (erro) {
            if (
                erro.message !==
                "Sua sessão expirou."
            ) {
                alert(erro.message);
            }
        } finally {
            elementos.botaoFinalizar.disabled =
                false;

            elementos.botaoFinalizar.textContent =
                "Finalizar compra";
        }
    }
);

// =====================================================
// ÁREA DO CLIENTE
// =====================================================

async function consultarDadosCliente() {
    if (!exigirLogin()) {
        return;
    }

    mostrarCarregamentoCliente(
        "Carregando dados..."
    );

    try {
        const resposta = await chamarApi(
            "/cliente/me"
        );

        if (!resposta.ok) {
            throw new Error(
                await lerMensagemErro(
                    resposta,
                    "Não foi possível consultar os dados."
                )
            );
        }

        const cliente = await resposta.json();

        mostrarResultadoCliente(`
            <article class="cliente-detalhe">
                <h3>
                    ${escaparHtml(
                        cliente.nomeCompleto
                    )}
                </h3>

                <p>
                    <strong>E-mail:</strong>
                    ${escaparHtml(cliente.email)}
                </p>

                <p>
                    <strong>CPF:</strong>
                    ${escaparHtml(cliente.cpf)}
                </p>

                <p>
                    <strong>Nascimento:</strong>
                    ${formatarData(
                        cliente.dataNascimento
                    )}
                </p>
            </article>
        `);
    } catch (erro) {
        mostrarErroCliente(erro.message);
    }
}

async function consultarEnderecos() {
    if (!exigirLogin()) {
        return;
    }

    mostrarCarregamentoCliente(
        "Carregando endereços..."
    );

    try {
        const resposta = await chamarApi(
            "/cliente/me/endereco"
        );

        if (!resposta.ok) {
            throw new Error(
                await lerMensagemErro(
                    resposta,
                    "Não foi possível consultar os endereços."
                )
            );
        }

        const enderecos = await resposta.json();

        if (!enderecos.length) {
            mostrarResultadoCliente(
                "<p>Nenhum endereço cadastrado.</p>"
            );

            return;
        }

        mostrarResultadoCliente(
            enderecos
                .map(endereco => `
                    <article class="cliente-detalhe">
                        <h3>
                            ${escaparHtml(
                                endereco.rua
                            )},
                            ${endereco.numero}
                        </h3>

                        <p>
                            ${escaparHtml(
                                endereco.bairro
                            )}
                        </p>

                        <p>
                            ${escaparHtml(
                                endereco.cidade
                            )}
                            -
                            ${escaparHtml(
                                endereco.estado
                            )}
                        </p>

                        <p>
                            <strong>CEP:</strong>
                            ${escaparHtml(
                                endereco.cep
                            )}
                        </p>
                    </article>
                `)
                .join("")
        );
    } catch (erro) {
        mostrarErroCliente(erro.message);
    }
}

async function consultarCartoes() {
    if (!exigirLogin()) {
        return;
    }

    mostrarCarregamentoCliente(
        "Carregando cartões..."
    );

    try {
        const resposta = await chamarApi(
            "/cliente/me/cartao"
        );

        if (!resposta.ok) {
            throw new Error(
                await lerMensagemErro(
                    resposta,
                    "Não foi possível consultar os cartões."
                )
            );
        }

        const cartoes = await resposta.json();

        if (!cartoes.length) {
            mostrarResultadoCliente(
                "<p>Nenhum cartão cadastrado.</p>"
            );

            return;
        }

        mostrarResultadoCliente(
            cartoes
                .map(cartao => `
                    <article class="cliente-detalhe">
                        <h3>
                            Cartão
                            ${escaparHtml(cartao.tipo)}
                        </h3>

                        <p>
                            Cadastrado em
                            ${formatarData(
                                cartao.dataCriacao
                            )}
                        </p>
                    </article>
                `)
                .join("")
        );
    } catch (erro) {
        mostrarErroCliente(erro.message);
    }
}

async function consultarPedidos() {
    if (!exigirLogin()) {
        return;
    }

    mostrarCarregamentoCliente(
        "Carregando pedidos..."
    );

    try {
        const resposta = await chamarApi(
            "/cliente/me/pedido"
        );

        if (!resposta.ok) {
            throw new Error(
                await lerMensagemErro(
                    resposta,
                    "Não foi possível consultar os pedidos."
                )
            );
        }

        const pedidos = await resposta.json();

        if (!pedidos.length) {
            mostrarResultadoCliente(
                "<p>Nenhum pedido encontrado.</p>"
            );

            return;
        }

        mostrarResultadoCliente(
            pedidos
                .map(pedido => `
                    <article class="cliente-detalhe">
                        <h3>
                            Pedido #${pedido.id}
                        </h3>

                        <p>
                            <strong>Status:</strong>
                            ${escaparHtml(
                                pedido.status
                            )}
                        </p>

                        <p>
                            <strong>Total:</strong>
                            ${formatarPreco(
                                pedido.valorTotal
                            )}
                        </p>

                        <ul>
                            ${pedido.itens
                                .map(item => `
                                    <li>
                                        ${escaparHtml(
                                            item.produtoNome
                                        )}
                                        ×
                                        ${item.quantidade}
                                        =
                                        ${formatarPreco(
                                            item.subtotal
                                        )}
                                    </li>
                                `)
                                .join("")}
                        </ul>
                    </article>
                `)
                .join("")
        );
    } catch (erro) {
        mostrarErroCliente(erro.message);
    }
}

// =====================================================
// EVENTOS
// =====================================================

elementos.botaoCarrinho.addEventListener(
    "click",
    abrirCarrinho
);

elementos.fecharCarrinho.addEventListener(
    "click",
    fecharCarrinhoPainel
);

elementos.overlay.addEventListener(
    "click",
    fecharCarrinhoPainel
);

elementos.fecharAutenticacao.addEventListener(
    "click",
    fecharModalAutenticacao
);

elementos.abaLogin.addEventListener(
    "click",
    () => selecionarAbaAutenticacao("login")
);

elementos.abaCadastro.addEventListener(
    "click",
    () => selecionarAbaAutenticacao("cadastro")
);

elementos.modalAutenticacao.addEventListener(
    "click",
    evento => {
        if (
            evento.target ===
            elementos.modalAutenticacao
        ) {
            fecharModalAutenticacao();
        }
    }
);

elementos.botaoConta.addEventListener(
    "click",
    evento => {
        evento.preventDefault();

        if (!exigirLogin()) {
            return;
        }

        $("cliente").scrollIntoView({
            behavior: "smooth"
        });

        consultarDadosCliente();
    }
);

elementos.botaoDadosCliente.addEventListener(
    "click",
    consultarDadosCliente
);

elementos.botaoEndereco.addEventListener(
    "click",
    consultarEnderecos
);

elementos.botaoPagamento.addEventListener(
    "click",
    consultarCartoes
);

elementos.botaoPedidos.addEventListener(
    "click",
    consultarPedidos
);

document.addEventListener(
    "keydown",
    evento => {
        if (evento.key === "Escape") {
            fecharCarrinhoPainel();
            fecharModalAutenticacao();
        }
    }
);

document.addEventListener(
    "DOMContentLoaded",
    () => {
        atualizarEstadoConta();
        atualizarCarrinho();
        carregarCategorias();
    }
);