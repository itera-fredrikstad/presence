<script lang="ts">
  import {
    QueryClient,
    QueryClientProvider,
  } from "@sveltestack/svelte-query";
  import Main from "./lib/components/Main.svelte";

  const queryClient = new QueryClient();
</script>

<QueryClientProvider client={queryClient}>
  <Main />
</QueryClientProvider>
