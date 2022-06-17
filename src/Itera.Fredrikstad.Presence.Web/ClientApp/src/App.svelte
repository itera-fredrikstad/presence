<script lang="ts">
  import { QueryClient, QueryClientProvider } from "@sveltestack/svelte-query";
  import Main from "./lib/components/Main.svelte";
  import DarkMode from "svelte-dark-mode";

  let theme;

  $: switchTheme = theme === "dark" ? "light" : "dark";
  $: document.body.className = theme;

  const queryClient = new QueryClient();
</script>

<DarkMode bind:theme />

<QueryClientProvider client={queryClient}>
  <Main switchTheme={() => (theme = switchTheme)} />
</QueryClientProvider>

<style>
  :global(.dark) {
    background: #333;
    color: #f1f8ff;
  }
</style>
