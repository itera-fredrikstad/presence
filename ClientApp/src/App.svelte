<script lang="ts">
  import { QueryClient, QueryClientProvider } from "@sveltestack/svelte-query";
  import Picker from "./lib/components/Picker.svelte";
  import Picker2 from "./lib/components/Picker2.svelte";
  import Summary from "./lib/components/Summary.svelte";

  import logo from "./assets/img/itera.png";

  const queryClient = new QueryClient();

  let page = document?.location?.hash;

  window.onpopstate = function () {
    page = document.location.hash;
  };
</script>

<main>
  <div class="navigation">
    <img class="logo" src={logo} alt="Logo" />
    <ul>
      <li>
        <a href="#oversikt">Oversikt</a>
      </li>
      <li>
        <a href="#registrering">Registrering</a>
      </li>
    </ul>
  </div>
  <QueryClientProvider client={queryClient}>
    <div class="content-root">
      {#if page === "#registrering"}
        <Picker />
      {:else}
        <Summary />
      {/if}
    </div>
  </QueryClientProvider>
</main>

<style>
  main {
    display: flex;
    width: 100%;
    height: 100%;
    align-items: center;
    justify-content: center;
    flex-direction: column;
  }

  .content-root {
    width: 60%;
  }

  .navigation {
    width: 100%;
    margin-bottom: 3rem;
    display: flex;
    justify-content: space-between;
    align-items: flex-start;
    margin-right: 5rem;
  }

  .navigation ul {
    display: flex;
  }

  .navigation li {
    list-style: none;
    margin-right: 1rem;
    text-transform: uppercase;
  }

  .navigation a {
    text-decoration: none;
    color: #333;
  }

  .logo {
    width: 56px;
    margin-left: 5rem;
  }
</style>
