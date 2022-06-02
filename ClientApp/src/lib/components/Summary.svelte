<script lang="ts">
  import { useQueryClient } from "@sveltestack/svelte-query";
  import {
    addWeeks,
    getMonth,
    format,
    eachDayOfInterval,
    isSaturday,
    isSunday,
    getISOWeek,
  } from "date-fns";
  import { nb } from "date-fns/locale";
  import SummaryDay from "./SummaryDay.svelte";

  let today = new Date();
  let offset = 0;

  $: allDays = eachDayOfInterval({
    start: today,
    end: addWeeks(today, offset + 2),
  });
  $: workDays = allDays.filter((day) => !isSaturday(day) && !isSunday(day));
  $: currentWindow = workDays.slice(offset * 5, (offset + 1) * 5);
  $: weeks = Object.keys(
    currentWindow.reduce((prev, next) => ({ ...prev, [getISOWeek(next)]: true }), {})
  );
  $: months = currentWindow.reduce<{ [key: string]: Date }>(
    (prev, next) => ({ ...prev, [getMonth(next)]: next }),
    {}
  );

  function handleNext() {
    offset++;
  }

  function handlePrev() {
    if (offset > 0) {
      offset--;
    }
  }
</script>

<div class="picker-root">
  <h1>Oversikt</h1>
  <div class="selector-wrapper">
    <h2>
      Uke {weeks.join("/")} ({Object.values(months)
        .map((d) => format(d, "MMMM", { locale: nb }))
        .join("/")})
    </h2>
  </div>
  <div class="wrapper">
    {#if offset > 0}
      <i class="fa-solid fa-circle-arrow-left" on:click={handlePrev} />
    {/if}
    <i class="fa-solid fa-circle-arrow-right" on:click={handleNext} />
    <div class="window">
      <div
        class="days slide"
        style="transform: translateX({offset * -100}%); transform-origin: 0 0"
      >
        {#each workDays as day}
          <SummaryDay {day} />
        {/each}
      </div>
    </div>
  </div>
</div>

<style>
  .picker-root {
    width: 100%;
  }

  .selector-wrapper {
    display: flex;
    align-items: center;
    justify-content: space-between;
  }

  h1 {
    font-size: 2rem;
  }

  h2 {
    font-size: 1.5rem;
  }

  .wrapper {
    position: relative;
    width: 100%;
  }

  .window {
    overflow: hidden;
    width: 100%;
  }

  .days {
    display: flex;
  }

  .slide {
    transition: all 0.5s;
  }

  .fa-solid {
    color: #999;
  }

  .fa-solid:hover {
    color: #333;
  }

  .fa-circle-arrow-right {
    position: absolute;
    right: -100px;
    top: 50%;
    transform: translateY(-50%);
    font-size: 2rem;
  }

  .fa-circle-arrow-left {
    position: absolute;
    left: -100px;
    top: 50%;
    transform: translateY(-50%);
    font-size: 2rem;
  }
</style>
