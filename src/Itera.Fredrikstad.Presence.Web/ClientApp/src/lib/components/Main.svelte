<script lang="ts">
  import logo from "../../assets/img/itera.png";

  import { useQuery, useQueryClient } from "@sveltestack/svelte-query";
  import Picker from "./Picker2.svelte";
  import Summary from "./Summary.svelte";
  import { getUser } from "../api";
  import SimpleAvatar from "./SimpleAvatar.svelte";

  let page = document?.location?.hash;
  export let switchTheme;

  $: query = useQuery(["user"], () => getUser(), {
    staleTime: Infinity,
    cacheTime: Infinity,
  });

  window.onpopstate = function () {
    page = document.location.hash;
  };
</script>

<main>
  <div class="navigation">
    <img
      class="logo"
      src={logo}
      alt="Itera logo"
      aria-label="Switch between dark mode and light mode"
      on:click={switchTheme}
    />
    <div class="menu">
      <ul>
        <li>
          <a href="#oversikt">Oversikt</a>
        </li>
        <li>
          <a href="#registrering">Registrering</a>
        </li>
      </ul>
      <span class="user">
        {#if $query.data}
          <SimpleAvatar user={$query.data} />
        {/if}
      </span>
    </div>
  </div>
  <div class="content-root">
    {#if page === "#registrering"}
      {#if !!$query.data?.userId}
        <Picker userId={$query.data.userId} />
      {:else}
        <h1>Laster...</h1>
      {/if}
    {:else}
      <Summary />
    {/if}
  </div>
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
    width: 80%;
  }

  .navigation {
    width: 100%;
    margin-bottom: 3rem;
    display: flex;
    justify-content: space-between;
    align-items: center;
    margin-right: 5rem;
  }

  .navigation ul {
    display: flex;
    margin: 0;
  }

  .navigation li {
    display: inline;
    list-style: none;
    margin-right: 1rem;
    text-transform: uppercase;
  }

  .navigation a {
    text-decoration: none;
    color: #333;
  }

  :global(.dark) .navigation a {
    color: white;
  }

  .logo {
    width: 56px;
    margin-left: 5rem;
    cursor: pointer;
  }

  .menu {
    display: flex;
    align-items: center;
  }
</style>
